<template>
    <div class="scorm-container">
        <header class="scorm-toolbar">
            <button class="back-btn" @click="handleBackClick">← Back</button>
            <div class="title">{{ title || "Course Player" }}</div>
            <div class="spacer"></div>
        </header>

        <div class="scorm-stage">
            <iframe :key="registrationId"
                    ref="iframeRef"
                    :src="launchUrl"
                    class="scorm-iframe"
                    allow="fullscreen"
                    title="SCORM Content" />
        </div>
    </div>
</template>

<script setup lang="js">import { ref, onMounted, onBeforeUnmount, defineProps, defineEmits } from "vue";
    import apiClient from "@/axios";

    const props = defineProps({
        launchUrl: { type: String, required: true },
        registrationId: { type: String, required: true },
        scoId: { type: String, required: true },
        preloadCmi: { type: Object, default: () => ({}) },
        title: { type: String, default: "" }
    });

    const emit = defineEmits(["exit"]);
    const iframeRef = ref(null);

    let commitInterval = null;
    let reinjectInterval = null;
    let uiWatchInterval = null;
    let bookmarkInterval = null;
    let mutationObserver = null;
    let exitClickCleanup = null;

    let beforeUnloadHandler = null;
    let pageHideHandler = null;
    let visibilityHandler = null;

    function safeLog(...args) {
        // eslint-disable-next-line no-console
        console.log("[SCORM]", ...args);
    }

    function readIframeTextSafe() {
        try {
            const iframe = iframeRef.value;
            const doc = iframe?.contentWindow?.document;
            if (!doc?.body) return "";
            return String(doc.body.innerText || "");
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] readIframeText blocked", e);
            return "";
        }
    }

    /**
     * Extracts "18% COMPLETE" etc from iframe text.
     * Returns 0..100 or null if not found.
     */
    function readPercentCompleteFromIframe() {
        const text = readIframeTextSafe();
        const m = text.match(/(\d{1,3})%\s*COMPLETE/i);
        if (!m) return null;
        const pct = parseInt(m[1], 10);
        if (Number.isNaN(pct)) return null;
        return Math.max(0, Math.min(100, pct));
    }

    function createScormApi({ registrationId, scoId, preload, onExit }) {
        let initialized = false;
        let lastError = "0";

        const cmi = new Map(Object.entries(preload || {}));
        const pending = new Map();

        let commitInFlight = false;
        let finishedOnce = false;

        // ✅ Track whether the PACKAGE itself sets these (so we don't override real bookmarking)
        let packageSetsLessonLocation = false;
        let packageSetsSuspendData = false;

        const setPending = (el, val) => {
            const v = String(val ?? "");
            pending.set(el, v);
            cmi.set(el, v);
        };

        function snapshotProgressToPending() {
            const keys = [
                "cmi.core.lesson_location",
                "cmi.suspend_data",
                "cmi.core.lesson_status",
                "cmi.core.score.raw",
                "cmi.completion_status",
                "cmi.success_status",
                "cmi.progress_measure",
                "cmi.completion_threshold"
            ];

            for (const k of keys) {
                const v = cmi.get(k);
                if (v !== undefined && v !== null && String(v).length > 0) {
                    setPending(k, v);
                }
            }
        }

        /**
         * ✅ Fallback bookmark:
         * If the package does NOT set lesson_location/suspend_data, store UI % there.
         * This allows resume if the package reads lesson_location on launch.
         */
        function maybeWriteBookmarkFromUiPercent() {
            const pct = readPercentCompleteFromIframe();
            if (pct === null) return;

            // do NOT touch if package already manages bookmarking
            if (!packageSetsLessonLocation) {
                setPending("cmi.core.lesson_location", String(pct));
            }

            if (!packageSetsSuspendData) {
                // keep small (suspend_data limit is 4096 in many implementations)
                const payload = JSON.stringify({ pct });
                setPending("cmi.suspend_data", payload);
            }

            safeLog("bookmark saved (fallback)", { pct, packageSetsLessonLocation, packageSetsSuspendData });
        }

        async function commitNowAsync() {
            if (commitInFlight) return;
            if (pending.size === 0) return;

            commitInFlight = true;

            const data = Array.from(pending.entries()).map(([element, value]) => ({ element, value }));
            pending.clear();

            try {
                safeLog("POST commit", { registrationId, scoId, count: data.length });
                await apiClient.post(`/scorm/runtime/${registrationId}/commit`, { scoId, data });
                lastError = "0";
            } catch (e) {
                lastError = "391";
                data.forEach(({ element, value }) => pending.set(element, value));
                // eslint-disable-next-line no-console
                console.debug("[SCORM] commit failed", e);
            } finally {
                commitInFlight = false;
            }
        }

        async function saveProgressNowAsync() {
            snapshotProgressToPending();
            await commitNowAsync();
        }

        const sessionStart = Date.now();
        function getSessionTime() {
            const totalSec = Math.floor((Date.now() - sessionStart) / 1000);
            const hh = String(Math.floor(totalSec / 3600)).padStart(4, "0");
            const mm = String(Math.floor((totalSec % 3600) / 60)).padStart(2, "0");
            const ss = String(totalSec % 60).padStart(2, "0");
            return `${hh}:${mm}:${ss}.00`;
        }

        async function finishNowAsync(clientCompleted = false) {
            if (finishedOnce) return;
            finishedOnce = true;

            await saveProgressNowAsync();

            try {
                safeLog("POST finish", { registrationId, scoId, clientCompleted });
                await apiClient.post(`/scorm/runtime/${registrationId}/finish`, {
                    scoId,
                    session_time: getSessionTime(),
                    client_completed: clientCompleted
                });
                lastError = "0";
            } catch (e) {
                lastError = "391";
                // eslint-disable-next-line no-console
                console.debug("[SCORM] finish failed", e);
            } finally {
                initialized = false;
            }
        }

        function forceCompleteCmiAndCommit() {
            setPending("cmi.core.lesson_status", "completed");
            setPending("cmi.completion_status", "completed");
            setPending("cmi.success_status", "passed");
        }

        async function exitSmartAsync() {
            // ✅ always store bookmark (fallback) BEFORE saving
            maybeWriteBookmarkFromUiPercent();
            await saveProgressNowAsync();

            // ✅ keep your existing completion behavior
            const pct = readPercentCompleteFromIframe();
            safeLog("exitSmart pct", pct);

            if (pct !== null && pct >= 100) {
                forceCompleteCmiAndCommit();
                await saveProgressNowAsync();
                await finishNowAsync(true);
            }
        }

        const API12 = {
            LMSInitialize: () => {
                safeLog("LMSInitialize()");
                if (initialized) {
                    lastError = "101";
                    return "false";
                }
                initialized = true;
                lastError = "0";
                return "true";
            },

            LMSFinish: () => {
                safeLog("LMSFinish()");
                lastError = "0";
                void exitSmartAsync().finally(() => onExit());
                return "true";
            },

            LMSGetValue: (element) => {
                if (!initialized) {
                    lastError = "301";
                    return "";
                }
                lastError = "0";
                const v = cmi.get(String(element)) ?? "";
                safeLog("LMSGetValue", element, "=>", v);
                return v;
            },

            LMSSetValue: (element, value) => {
                const el = String(element);
                const valStr = String(value ?? "");

                if (el === "cmi.suspend_data" && valStr.length > 4096) {
                    lastError = "405";
                    return "false";
                }

                // ✅ detect if package manages bookmarking
                if (el === "cmi.core.lesson_location") packageSetsLessonLocation = true;
                if (el === "cmi.suspend_data") packageSetsSuspendData = true;

                setPending(el, value);
                lastError = "0";
                safeLog("LMSSetValue", el, "=", valStr);
                return "true";
            },

            LMSCommit: () => {
                safeLog("LMSCommit()");
                lastError = "0";
                void commitNowAsync();
                return "true";
            },

            LMSGetLastError: () => lastError,
            LMSGetErrorString: (code) => String(code),
            LMSGetDiagnostic: (code) => String(code),

            ConcedeControl: () => {
                safeLog("ConcedeControl()");
                void exitSmartAsync().finally(() => onExit());
                return "true";
            },
            Finish: () => {
                safeLog("Finish()");
                void exitSmartAsync().finally(() => onExit());
                return "true";
            },
            CommitData: () => {
                safeLog("CommitData()");
                void commitNowAsync();
                return "true";
            }
        };

        const API2004 = {
            Initialize: () => API12.LMSInitialize(),
            Terminate: () => API12.LMSFinish(),
            GetValue: (el) => API12.LMSGetValue(el),
            SetValue: (el, v) => API12.LMSSetValue(el, v),
            Commit: () => API12.LMSCommit(),
            GetLastError: () => API12.LMSGetLastError(),
            GetErrorString: (c) => API12.LMSGetErrorString(c),
            GetDiagnostic: (c) => API12.LMSGetDiagnostic(c)
        };

        function sendBeaconCommitBestEffort() {
            try {
                if (!navigator?.sendBeacon) return;
                if (pending.size === 0) return;

                const data = Array.from(pending.entries()).map(([element, value]) => ({ element, value }));
                pending.clear();

                const blob = new Blob([JSON.stringify({ scoId, data })], { type: "application/json" });
                navigator.sendBeacon(`/api/scorm/runtime/${registrationId}/commit`, blob);

                safeLog("sendBeacon commit", data.length);
            } catch (e) {
                // eslint-disable-next-line no-console
                console.debug("[SCORM] sendBeacon failed", e);
            }
        }

        return {
            API12,
            API2004,
            commitNowAsync,
            saveProgressNowAsync,
            finishNowAsync,
            exitSmartAsync,
            sendBeaconCommitBestEffort,
            maybeWriteBookmarkFromUiPercent
        };
    }

    const api = createScormApi({
        registrationId: props.registrationId,
        scoId: props.scoId,
        preload: props.preloadCmi,
        onExit: () => emit("exit")
    });

    function setApiOnWindow(win) {
        try {
            if (!win) return;
            win.API = api.API12;
            win.API_1484_11 = api.API2004;
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] setApiOnWindow failed", e);
        }
    }

    function injectIntoAllFrames(win, depth = 0) {
        if (!win || depth > 12) return;
        setApiOnWindow(win);

        try {
            const frames = win.frames;
            for (let i = 0; i < frames.length; i += 1) {
                injectIntoAllFrames(frames[i], depth + 1);
            }
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] injectIntoAllFrames blocked", e);
        }
    }

    function reinjectEverywhere() {
        setApiOnWindow(window);
        setApiOnWindow(window.parent);
        setApiOnWindow(window.top);

        const iframe = iframeRef.value;
        if (!iframe) return;

        try {
            injectIntoAllFrames(iframe.contentWindow, 0);
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] reinject failed", e);
        }
    }

    function hookExitCourseClicks() {
        const iframe = iframeRef.value;
        if (!iframe) return;

        try {
            const doc = iframe.contentWindow?.document;
            if (!doc) return;

            const clickHandler = (ev) => {
                const t = ev.target;
                const text = String(t?.innerText || t?.textContent || "").trim().toUpperCase();
                if (text.includes("EXIT COURSE")) {
                    safeLog("EXIT COURSE clicked -> exitSmart");
                    void api.exitSmartAsync().finally(() => emit("exit"));
                }
            };

            doc.addEventListener("click", clickHandler, true);
            exitClickCleanup = () => {
                try {
                    doc.removeEventListener("click", clickHandler, true);
                } catch (e) {
                    // eslint-disable-next-line no-console
                    console.debug("[SCORM] remove click handler failed", e);
                }
            };

            mutationObserver = new MutationObserver(() => { });
            mutationObserver.observe(doc.documentElement, { childList: true, subtree: true });

            safeLog("Exit click hook installed");
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] hookExitCourseClicks failed", e);
        }
    }

    async function handleBackClick() {
        try {
            await api.exitSmartAsync();
        } finally {
            emit("exit");
        }
    }

    onMounted(() => {
        window.API = api.API12;
        window.API_1484_11 = api.API2004;

        reinjectEverywhere();
        reinjectInterval = setInterval(reinjectEverywhere, 1000);

        // periodic commit
        commitInterval = setInterval(() => void api.commitNowAsync(), 30_000);

        // ✅ NEW: periodic bookmark save (writes lesson_location/suspend_data fallback)
        bookmarkInterval = setInterval(() => {
            api.maybeWriteBookmarkFromUiPercent();
            void api.commitNowAsync();
        }, 15_000);

        // completion watcher (your existing fallback)
        uiWatchInterval = setInterval(() => {
            const pct = readPercentCompleteFromIframe();
            if (pct === null) return;

            safeLog("UI %", pct);

            if (pct >= 100) {
                safeLog("UI reached 100% -> finishing");
                void api.finishNowAsync(true);
                clearInterval(uiWatchInterval);
                uiWatchInterval = null;
            }
        }, 5000);

        setTimeout(hookExitCourseClicks, 1500);

        const commitQuick = () => {
            api.maybeWriteBookmarkFromUiPercent();
            void api.saveProgressNowAsync();
            api.sendBeaconCommitBestEffort();
        };

        beforeUnloadHandler = () => commitQuick();
        pageHideHandler = () => commitQuick();
        visibilityHandler = () => {
            if (document.visibilityState === "hidden") commitQuick();
        };

        window.addEventListener("beforeunload", beforeUnloadHandler);
        window.addEventListener("pagehide", pageHideHandler);
        document.addEventListener("visibilitychange", visibilityHandler);
    });

    onBeforeUnmount(async () => {
        try {
            api.maybeWriteBookmarkFromUiPercent();
            await api.saveProgressNowAsync();
            api.sendBeaconCommitBestEffort();
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] unmount save failed", e);
        }

        if (commitInterval) clearInterval(commitInterval);
        if (reinjectInterval) clearInterval(reinjectInterval);
        if (uiWatchInterval) clearInterval(uiWatchInterval);
        if (bookmarkInterval) clearInterval(bookmarkInterval);
        if (mutationObserver) mutationObserver.disconnect();
        if (exitClickCleanup) exitClickCleanup();

        if (beforeUnloadHandler) window.removeEventListener("beforeunload", beforeUnloadHandler);
        if (pageHideHandler) window.removeEventListener("pagehide", pageHideHandler);
        if (visibilityHandler) document.removeEventListener("visibilitychange", visibilityHandler);

        try {
            delete window.API;
            delete window.API_1484_11;
        } catch (e) {
            // eslint-disable-next-line no-console
            console.debug("[SCORM] cleanup failed", e);
        }
    });</script>

<style scoped>
    .scorm-container {
        display: flex;
        flex-direction: column;
        height: 100%;
        background: #fff;
        border-radius: 16px;
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
        overflow: hidden;
    }

    .scorm-toolbar {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 10px 14px;
        border-bottom: 1px solid #eee;
        background: #f9fafb;
    }

    .back-btn {
        border: none;
        background: #e5e7eb;
        padding: 8px 12px;
        border-radius: 10px;
        font-weight: 600;
        cursor: pointer;
    }

    .title {
        font-weight: 700;
    }

    .spacer {
        flex: 1;
    }

    .scorm-stage {
        flex: 1;
        min-height: 60vh;
        max-height: calc(100vh - 60px);
    }

    .scorm-iframe {
        width: 100%;
        height: calc(100vh - 56px);
        border: 0;
    }
</style>