<template>
    <div class="calendar-wrapper">
        <h1 class="calendar-title">Training Calendar</h1>

        <!-- ✅ keep only ONE calendar -->
        <FullCalendar ref="cal" class="fc-theme-standard" :options="calendarOptions" />

        <!-- ✅ Course modal -->
        <CourseDetailModal v-if="selectedCourse"
                           :course="selectedCourse"
                           @register="handleRegister"
                           @request-login="showLoginModal = true"
                           @close="selectedCourse = null" />

        <!-- ✅ Custom Event modal -->
        <CustomEventModal v-if="selectedCustomEvent"
                          :event="selectedCustomEvent"
                          @close="selectedCustomEvent = null" />

        <SuccessModal v-if="showSuccessModal"
                      :message="successMessage"
                      :email="user?.email || ''"
                      @close="showSuccessModal = false" />

        <LoginComponent v-if="showLoginModal"
                        @login-success="handleLoginSuccess"
                        @close="showLoginModal = false"
                        @show-register="handleShowRegister" />

        <RegisterComponent v-if="showRegisterModal"
                           @close="showRegisterModal = false"
                           @register-success="handleRegisterSuccess" />
    </div>
</template>

<script>import FullCalendar from "@fullcalendar/vue3";
    import dayGridPlugin from "@fullcalendar/daygrid";
    import timeGridPlugin from "@fullcalendar/timegrid";
    import interactionPlugin from "@fullcalendar/interaction";

    import tippy from "tippy.js";
    import "tippy.js/dist/tippy.css";

    // ✅ use your configured axios client (baseURL ends with /api)
    import apiClient from "@/axios";

    import CourseDetailModal from "@/components/Modals/CourseDetailModal.vue";
    import CustomEventModal from "@/components/Modals/CustomEventModal.vue";
    import SuccessModal from "@/components/Modals/SuccessModal.vue";
    import LoginComponent from "@/components/LoginComponent.vue";
    import RegisterComponent from "@/components/RegistrationModal.vue";

    export default {
        name: "TrainingCalendar",
        components: {
            FullCalendar,
            CourseDetailModal,
            CustomEventModal,
            SuccessModal,
            LoginComponent,
            RegisterComponent,
        },
        data() {
            return {
                // modal state
                selectedCourse: null,
                selectedCustomEvent: null,

                user: null,
                showSuccessModal: false,
                successMessage: "",
                showLoginModal: false,
                showRegisterModal: false,

                calendarOptions: {
                    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
                    initialView: "dayGridMonth",
                    headerToolbar: {
                        left: "prev,next today",
                        center: "title",
                        right: "dayGridMonth,timeGridWeek,timeGridDay",
                    },
                    height: "auto",
                    editable: false,
                    timeZone: "local",

                    // ✅ keep these while you test
                    datesSet: (arg) => {
                        console.log("✅ datesSet fired", {
                            start: arg.startStr,
                            end: arg.endStr,
                            view: arg.view?.type,
                        });
                    },
                    loading: (isLoading) => {
                        console.log("⏳ calendar loading:", isLoading);
                    },

                    /**
                     * ✅ Two sources:
                     * 1) Courses feed (existing)
                     * 2) Custom events feed (new)
                     */
                    eventSources: [
                        {
                            id: "training-source",
                            events: async (info, successCallback, failureCallback) => {
                                try {
                                    const res = await apiClient.get("/TrainingCalendar/events", {
                                        params: { start: info.startStr, end: info.endStr },
                                    });

                                    const events = Array.isArray(res.data)
                                        ? res.data
                                        : Array.isArray(res.data?.$values)
                                            ? res.data.$values
                                            : [];

                                    // Ensure a type flag exists for click/tooltip logic
                                    const normalized = events.map((e) => ({
                                        ...e,
                                        extendedProps: {
                                            ...(e.extendedProps || {}),
                                            __type: "course",
                                        },
                                    }));

                                    successCallback(normalized);
                                } catch (err) {
                                    console.error("❌ Course calendar load failed:", err?.response?.data || err);
                                    failureCallback(err);
                                }
                            },
                        },
                        {
                            id: "custom-source",
                            events: async (info, successCallback, failureCallback) => {
                                try {
                                    // ✅ Your new controller endpoint
                                    // Example: GET /api/CustomCalendarEvents/calendar?start=...&end=...
                                    const res = await apiClient.get("/CustomCalendarEvents/calendar", {
                                        params: { start: info.startStr, end: info.endStr },
                                    });

                                    const events = Array.isArray(res.data)
                                        ? res.data
                                        : Array.isArray(res.data?.$values)
                                            ? res.data.$values
                                            : [];

                                    // Normalize: mark as custom + keep id/title/start/end fields
                                    const normalized = events.map((e) => ({
                                        ...e,
                                        // If backend returns id like "custom-12", keep it.
                                        // If backend returns numeric id, keep it too.
                                        extendedProps: {
                                            ...(e.extendedProps || {}),
                                            __type: "custom",
                                            // Helpful: allow click to load details if needed
                                            customEventId:
                                                e.extendedProps?.customEventId ??
                                                e.customEventId ??
                                                e.id, // fallback
                                        },
                                    }));

                                    successCallback(normalized);
                                } catch (err) {
                                    console.error("❌ Custom events load failed:", err?.response?.data || err);
                                    failureCallback(err);
                                }
                            },
                        },
                    ],

                    // ✅ Hover tooltip
                    eventMouseEnter: (info) => {
                        const ep = info.event.extendedProps || {};
                        const type = ep.__type || "course";
                        const title = info.event.title || "Event";

                        let html = "";

                        if (type === "custom") {
                            // Custom event tooltip (adjust keys to match your API response)
                            // Common fields: name/title, shortDescription, description, location, link
                            const shortDesc = ep.shortDescription || ep.description || "";
                            const location = ep.location || "";
                            const link = ep.linkUrl || ep.url || "";

                            html = `
              <div style="font-size:13px; line-height:1.35; max-width:320px">
                <div style="font-weight:700; margin-bottom:6px">${escapeHtml(title)}</div>
                ${location ? `<div><b>Location:</b> ${escapeHtml(location)}</div>` : ""}
                ${shortDesc ? `<div style="margin-top:6px">${escapeHtml(shortDesc)}</div>` : ""}
                ${link ? `<div style="margin-top:6px"><b>Link:</b> ${escapeHtml(link)}</div>` : ""}
              </div>
            `;
                        } else {
                            // Course tooltip (your existing)
                            const city = ep.city || "";
                            const trainingLocation = ep.trainingLocation || "";
                            const virtualUrl = ep.virtualUrl || "";

                            html = `
              <div style="font-size:13px; line-height:1.3; max-width:320px">
                <div style="font-weight:700; margin-bottom:6px">${escapeHtml(title)}</div>
                ${city ? `<div><b>City:</b> ${escapeHtml(city)}</div>` : ""}
                ${trainingLocation ? `<div><b>Location:</b> ${escapeHtml(trainingLocation)}</div>` : ""}
                ${virtualUrl ? `<div style="margin-top:6px"><b>Link:</b> ${escapeHtml(virtualUrl)}</div>` : ""}
              </div>
            `;
                        }

                        info.el._tippy = tippy(info.el, {
                            content: html,
                            allowHTML: true,
                            placement: "top",
                            interactive: true,
                            appendTo: document.body,
                        });

                        info.el._tippy.show();
                    },

                    eventMouseLeave: (info) => {
                        if (info.el._tippy) {
                            info.el._tippy.destroy();
                            info.el._tippy = null;
                        }
                    },

                    // ✅ Click handling: course vs custom
                    eventClick: async (clickInfo) => {
                        clickInfo.jsEvent.preventDefault();

                        const ep = clickInfo?.event?.extendedProps || {};
                        const type = ep.__type || "course";

                        // Ctrl/⌘ + click open a URL if present
                        const ctrl = clickInfo.jsEvent.ctrlKey || clickInfo.jsEvent.metaKey;

                        const maybeUrl =
                            type === "course"
                                ? ep.virtualUrl
                                : ep.linkUrl || ep.url;

                        if (ctrl && maybeUrl) {
                            window.open(maybeUrl, "_blank", "noopener");
                            return;
                        }

                        // close tooltip if open
                        if (clickInfo.el?._tippy) {
                            clickInfo.el._tippy.destroy();
                            clickInfo.el._tippy = null;
                        }

                        if (type === "custom") {
                            // Option A: if your calendar feed already returns full details, just open it
                            // Option B: fetch full details by id, then open
                            const customId = ep.customEventId;

                            try {
                                // If you have a GET by id endpoint, use it:
                                // GET /api/CustomCalendarEvents/{id}
                                const res = await apiClient.get(`/CustomCalendarEvents/${customId}`);
                                this.selectedCustomEvent = res.data;
                            } catch (err) {
                                // fallback: open with what we already have
                                console.warn("Custom event details fetch failed, using calendar payload", err);
                                this.selectedCustomEvent = {
                                    title: clickInfo.event.title,
                                    start: clickInfo.event.startStr,
                                    end: clickInfo.event.endStr,
                                    ...ep,
                                };
                            }
                            return;
                        }

                        // default: course
                        const courseSysId = ep.courseSysId;
                        if (!courseSysId) return;

                        try {
                            const res = await apiClient.get(`/Course/${courseSysId}`);
                            this.selectedCourse = res.data;
                        } catch (err) {
                            console.error("Failed to load course details:", err?.response?.data || err);
                        }
                    },
                },
            };
        },
        mounted() {
            console.log("training calendar mounted ✅");
            setTimeout(() => {
                this.$refs.cal?.getApi()?.refetchEvents();
            }, 200);

            this.fetchUser();
        },
        methods: {
            async fetchUser() {
                const userId = localStorage.getItem("userId");
                if (!userId) return;

                try {
                    const res = await apiClient.get(`/user/${userId}`);
                    this.user = res.data;
                } catch (err) {
                    console.error("Failed to fetch user:", err?.response?.data || err);
                }
            },

            handleShowRegister() {
                this.showLoginModal = false;
                this.showRegisterModal = true;
            },

            handleRegisterSuccess() {
                this.showRegisterModal = false;
                this.fetchUser();
            },

            handleLoginSuccess(userData) {
                localStorage.setItem("userId", userData.userId);
                localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
                localStorage.setItem("jwtToken", userData.token);
                this.showLoginModal = false;

                if (this.selectedCourse) {
                    this.handleRegister(this.selectedCourse, true);
                }
            },

            async handleRegister(course, isFromLogin = false) {
                try {
                    const userId = localStorage.getItem("userId");
                    if (!userId) {
                        this.showLoginModal = true;
                        return;
                    }

                    const res = await apiClient.post("/Course/register", {
                        userId,
                        courseId: course.courseSysId,
                        adaneed: course.adaneed || false,
                        adadetails: course.adadetails || "",
                    });

                    this.successMessage = res.data?.message || "Registration successful.";
                    this.showSuccessModal = true;

                    if (!isFromLogin) this.selectedCourse = null;
                } catch (err) {
                    console.error("Registration failed:", err?.response?.data || err);
                }
            },
        },
    };

    function escapeHtml(str) {
        return String(str)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }</script>

<style scoped>
    .calendar-wrapper {
        width: 100%;
        padding: 30px;
        box-sizing: border-box;
    }

    .calendar-title {
        text-align: center;
        font-size: 2rem;
        font-weight: bold;
        color: #6e528d;
        margin-bottom: 20px;
    }

    :deep(.fc) {
        background: #ffffff;
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 4px 18px rgba(0, 0, 0, 0.08);
    }

    :deep(.fc-toolbar-title) {
        font-size: 1.4rem;
        font-weight: 600;
        color: #444;
    }

    :deep(.fc-button) {
        background: #6e528d !important;
        border: none !important;
        color: white !important;
        padding: 6px 14px !important;
        border-radius: 6px !important;
        font-size: 0.9rem !important;
        cursor: pointer !important;
    }

    :deep(.fc-button:hover) {
        background: #593d72 !important;
    }
</style>