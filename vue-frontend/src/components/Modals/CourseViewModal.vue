<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <header class="modal-header">
                <div>
                    <h2>Course Details</h2>
                    <p class="modal-subtitle">Schedule, location, and online access information.</p>
                </div>
                <span class="modal-badge">{{ formatText }}</span>
            </header>

            <!-- Loading -->
            <section class="section-card" v-if="loading">
                <div class="section-header">
                    <h3>Loading…</h3>
                    <p class="modal-subtitle">Fetching full course details.</p>
                </div>
            </section>

            <!-- 1) COURSE INFO -->
            <section class="section-card" v-else>
                <div class="section-header">
                    <h3>{{ courseData?.subjectTitle || courseData?.SubjectTitle || "N/A" }}</h3>
                    <!-- NO DESCRIPTION -->
                </div>

                <div class="info-grid">
                    <div class="info-item">
                        <label>Date</label>
                        <div class="value">{{ dateRangeText }}</div>
                    </div>

                    <div class="info-item">
                        <label>Time</label>
                        <div class="value">{{ timeText }}</div>
                    </div>

                    <div class="info-item">
                        <label>Format</label>
                        <div class="value">{{ formatText }}</div>
                    </div>

                    <div class="info-item" v-if="courseData?.regDeadLine || courseData?.RegDeadLine">
                        <label>Registration Deadline</label>
                        <div class="value">{{ formatDate(courseData?.regDeadLine || courseData?.RegDeadLine) }}</div>
                    </div>

                    <div class="info-item" v-if="courseData?.maxSeats || courseData?.MaxSeats">
                        <label>Max Seats</label>
                        <div class="value">{{ courseData?.maxSeats ?? courseData?.MaxSeats }}</div>
                    </div>

                    <div class="info-item" v-if="isMultiSession">
                        <label>Multi-session</label>
                        <div class="value">Yes — must attend all sessions</div>
                    </div>
                </div>
            </section>

            <!-- 2) LOCATION (separate section) -->
            <section class="section-card" v-if="hasAnyLocation && !loading">
                <div class="section-header">
                    <h3>Location</h3>
                    <p class="modal-subtitle">Where the training will be held.</p>
                </div>

                <div class="info-grid">
                    <div class="info-item">
                        <label>Training Location</label>
                        <div class="value">{{ trainingLocationText }}</div>
                    </div>

                    <div class="info-item">
                        <label>City</label>
                        <div class="value">{{ cityText }}</div>
                    </div>

                    <div class="info-item" v-if="courseData?.siteName || courseData?.SiteName">
                        <label>Training Center</label>
                        <div class="value">{{ courseData?.siteName || courseData?.SiteName }}</div>
                    </div>
                </div>
            </section>

            <!-- 3) ONLINE ACCESS (VirtualUrl from Courses table ONLY) -->
            <section class="section-card" v-if="hasVirtualUrl && !loading">
                <div class="section-header">
                    <h3>Online Access</h3>
                    <p class="modal-subtitle">Use this link to join the session.</p>
                </div>

                <div class="link-row">
                    <a class="link-pill" :href="normalizedVirtualUrl" target="_blank" rel="noopener">
                        Open Virtual Link
                    </a>
                    <button class="btn-secondary" type="button" @click="copyText(normalizedVirtualUrl)">
                        Copy
                    </button>
                </div>

                <div class="url-preview">{{ normalizedVirtualUrl }}</div>
            </section>

            <!-- 4) SESSIONS (BOTTOM) - only if multi-session -->
            <section class="section-card" v-if="isMultiSession && sessionsList.length && !loading">
                <div class="section-header">
                    <h3>Session Schedule</h3>
                    <p class="modal-subtitle">You must attend all sessions.</p>
                </div>

                <div class="session-list">
                    <div class="session-card" v-for="(s, i) in sessionsList" :key="i">
                        <div class="session-title">Session {{ i + 1 }}</div>

                        <div class="info-grid">
                            <div class="info-item">
                                <label>Date</label>
                                <div class="value">{{ formatDate(s.sessionDate || s.SessionDate || s.date || s.Date) }}</div>
                            </div>

                            <div class="info-item">
                                <label>Start Time</label>
                                <div class="value">{{ formatTime(s.startTime || s.StartTime) }}</div>
                            </div>

                            <div class="info-item">
                                <label>End Time</label>
                                <div class="value">{{ formatTime(s.endTime || s.EndTime) }}</div>
                            </div>

                            <div class="info-item">
                                <label>Training Location</label>
                                <div class="value">{{ (s.trainingLocation || s.TrainingLocation || "N/A") }}</div>
                            </div>
                        </div>

                        <div v-if="sessionUrlValue(s)" class="link-row" style="margin-top: 10px;">
                            <a class="link-pill" :href="normalizedSessionUrl(s)" target="_blank" rel="noopener">
                                Open Session Link
                            </a>
                            <button class="btn-secondary" type="button" @click="copyText(normalizedSessionUrl(s))">
                                Copy
                            </button>
                        </div>

                        <div v-if="sessionUrlValue(s)" class="url-preview">
                            {{ normalizedSessionUrl(s) }}
                        </div>
                    </div>
                </div>
            </section>

            <div class="button-group">
                <button class="btn-secondary" @click="$emit('close')">Close</button>
            </div>
        </div>
    </div>
</template>
<script>import apiClient from "@/axios";

    export default {
        name: "CourseViewModal",
        emits: ["close"],
        props: {
            course: { type: Object, default: null } // list item
        },
        data() {
            return {
                loading: false,
                fullCourse: null
            };
        },
        computed: {
            courseData() {
                return this.fullCourse || this.course;
            },
            courseId() {
                return this.course?.courseSysId ?? this.course?.CourseSysId ?? 0;
            },

            // Multi-session
            isMultiSession() {
                return !!(this.courseData?.isMultiSession ?? this.courseData?.IsMultiSession);
            },

            // Sessions list (supports both { Sessions: [...] } and { sessions: {$values:[]} } shapes)
            sessionsList() {
                const c = this.courseData;
                const s = c?.sessions ?? c?.Sessions;

                // If backend returns "$values"
                if (s && s.$values && Array.isArray(s.$values)) return s.$values;

                // If backend returns List<T>
                if (Array.isArray(s)) return s;

                return [];
            },

            // ----- LOCATION -----
            trainingLocationText() {
                const c = this.courseData;
                return (c?.trainingLocation ?? c?.TrainingLocation ?? "").toString().trim() || "N/A";
            },
            cityText() {
                const c = this.courseData;
                return (c?.city ?? c?.City ?? "").toString().trim() || "N/A";
            },
            hasAnyLocation() {
                return (
                    (this.trainingLocationText && this.trainingLocationText !== "N/A") ||
                    (this.cityText && this.cityText !== "N/A") ||
                    !!(this.courseData?.siteName || this.courseData?.SiteName)
                );
            },

            // ----- ONLINE ACCESS (Courses.VirtualUrl ONLY) -----
            virtualUrlValue() {
                const c = this.courseData;
                return (c?.virtualUrl ?? c?.VirtualUrl ?? "").toString().trim();
            },
            hasVirtualUrl() {
                return !!this.virtualUrlValue;
            },
            normalizedVirtualUrl() {
                const raw = this.virtualUrlValue;
                if (!raw) return "";
                return /^https?:\/\//i.test(raw) ? raw : `https://${raw}`;
            },

            // ----- DISPLAY -----
            dateRangeText() {
                const c = this.courseData;
                const start = c?.courseDate ?? c?.CourseDate;
                const end = c?.endDate ?? c?.EndDate;
                if (!start) return "N/A";
                const s = new Date(start).toLocaleDateString();
                if (!end) return s;
                const e = new Date(end).toLocaleDateString();
                return s === e ? s : `${s} - ${e}`;
            },
            timeText() {
                const c = this.courseData;
                return ((c?.courseTime ?? c?.CourseTime ?? "").toString().trim()) || "N/A";
            },
            formatText() {
                const c = this.courseData;
                return (c?.formatLabel ?? c?.FormatLabel ?? c?.format ?? c?.Format ?? "Training");
            }
        },

        async mounted() {
            await this.fetchFullCourse();
        },
        watch: {
            courseId: {
                async handler() {
                    await this.fetchFullCourse();
                }
            }
        },

        methods: {
            async fetchFullCourse() {
                this.fullCourse = null;
                if (!this.courseId) return;

                this.loading = true;
                try {
                    const res = await apiClient.get(`/Course/${this.courseId}`);
                    this.fullCourse = res.data || null;
                } catch (e) {
                    console.error("Failed to load full course details:", e);
                } finally {
                    this.loading = false;
                }
            },

            // SessionUrl helpers
            sessionUrlValue(session) {
                return (session?.sessionUrl ?? session?.SessionUrl ?? "").toString().trim();
            },
            normalizedSessionUrl(session) {
                const raw = this.sessionUrlValue(session);
                if (!raw) return "";
                return /^https?:\/\//i.test(raw) ? raw : `https://${raw}`;
            },

            formatDate(d) {
                if (!d) return "N/A";
                return new Date(d).toLocaleDateString();
            },
            formatTime(t) {
                if (!t) return "N/A";
                // supports "HH:mm:ss" or "HH:mm"
                const str = t.toString();
                const [hh, mm] = str.split(":");
                if (!hh || !mm) return str;
                const dt = new Date();
                dt.setHours(Number(hh), Number(mm), 0, 0);
                return dt.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
            },

            async copyText(text) {
                if (!text) return;
                try {
                    await navigator.clipboard.writeText(text);
                    alert("Copied!");
                } catch {
                    alert("Unable to copy link.");
                }
            }
        }
    };</script>


<style scoped>
    /* OVERLAY */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(circle at top, rgba(15,23,42,0.3), rgba(15,23,42,0.75));
        backdrop-filter: blur(6px);
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 24px;
        z-index: 999;
    }

    /* MODAL */
    .modal {
        position: relative;
        background: #ffffff;
        border-radius: 24px;
        width: 900px;
        max-width: 100%;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15,23,42,0.25), 0 0 0 1px rgba(148,163,184,0.35);
    }

    /* CLOSE */
    .close-btn {
        position: absolute;
        top: 14px;
        right: 16px;
        background: rgba(255,255,255,0.9);
        border: none;
        border-radius: 50%;
        width: 34px;
        height: 34px;
        font-size: 20px;
        cursor: pointer;
    }

    /* HEADER */
    .modal-header {
        background: #43285D;
        color: white;
        margin: -32px -32px 24px -32px;
        padding: 28px 40px;
        border-top-left-radius: 24px;
        border-top-right-radius: 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .modal-header h2 {
            font-size: 28px;
            font-weight: 700;
            margin: 0;
        }

    .modal-subtitle {
        font-size: 14px;
        opacity: 0.9;
        margin-top: 6px;
    }

    .modal-badge {
        background: rgba(255,255,255,0.15);
        padding: 8px 18px;
        border-radius: 999px;
        font-weight: 600;
        font-size: 12px;
        color: white;
    }

    /* SECTION */
    .section-card {
        background: white;
        border-radius: 18px;
        padding: 20px;
        border: 1px solid #e5e7eb;
        margin-bottom: 20px;
        box-shadow: 0 12px 30px rgba(15,23,42,0.08);
    }

    .section-header h3 {
        margin: 0 0 6px 0;
        font-size: 18px;
    }

    .section-header p {
        margin: 0;
        color: #555;
    }

    /* INFO GRID */
    .info-grid {
        display: grid;
        grid-template-columns: repeat(3, minmax(0, 1fr));
        gap: 14px;
        margin-top: 16px;
    }

    .info-item label {
        display: block;
        font-size: 12px;
        color: #6b7280;
        margin-bottom: 4px;
    }

    .info-item .value {
        font-weight: 600;
        color: #111827;
    }

    /* BUTTONS */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 20px;
    }

    .btn-primary,
    .btn-secondary {
        transition: all 0.25s ease;
        transform: translateY(0);
    }

    .btn-primary {
        background: #43285D;
        color: white;
        padding: 10px 22px;
        border: none;
        border-radius: 999px;
        font-weight: 600;
        box-shadow: 0 4px 12px rgba(67, 40, 93, 0.22);
    }

        .btn-primary:hover {
            background: #361F4A;
            transform: translateY(-3px);
            box-shadow: 0 8px 18px rgba(67, 40, 93, 0.32);
        }

    .btn-secondary {
        background: #e5e7eb;
        color: #333;
        padding: 10px 22px;
        border-radius: 999px;
        border: none;
        font-weight: 500;
        box-shadow: 0 2px 6px rgba(0,0,0,0.10);
    }

        .btn-secondary:hover {
            background: #d4d4d4;
            transform: translateY(-3px);
            box-shadow: 0 6px 14px rgba(0,0,0,0.15);
        }

    @media (max-width: 768px) {
        .info-grid {
            grid-template-columns: 1fr;
        }
    }
</style>