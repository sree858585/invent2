<template>
    <div class="calendar-wrapper">
        <h1 class="calendar-title">Training Calendar</h1>

        <FullCalendar class="fc-theme-standard" :options="calendarOptions" />

        <!-- ✅ Course modal (same one used in Course List page) -->
        <CourseDetailModal v-if="selectedCourse"
                           :course="selectedCourse"
                           @register="handleRegister"
                           @request-login="showLoginModal = true"
                           @close="selectedCourse = null" />

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
    import axios from "axios";

    import tippy from "tippy.js";
    import "tippy.js/dist/tippy.css";

    // ✅ Use same api client as Course List page (baseURL, interceptors, JWT etc.)
    import apiClient from "@/axios";

    // ✅ Same modals as Course List page
    import CourseDetailModal from "@/components/Modals/CourseDetailModal.vue";
    import SuccessModal from "@/components/Modals/SuccessModal.vue";
    import LoginComponent from "@/components/LoginComponent.vue";
    import RegisterComponent from "@/components/RegistrationModal.vue";

    export default {
        name: "TrainingCalendar",
        components: {
            FullCalendar,
            CourseDetailModal,
            SuccessModal,
            LoginComponent,
            RegisterComponent,
        },
        data() {
            return {
                // modal state
                selectedCourse: null,
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

                    // ✅ Load calendar events
                    events: async (info, successCallback, failureCallback) => {
                        try {
                            const res = await axios.get("/api/TrainingCalendar/events", {
                                params: { start: info.startStr, end: info.endStr },
                            });
                            successCallback(Array.isArray(res.data) ? res.data : []);
                        } catch (err) {
                            console.error("Calendar load failed:", err?.response?.data || err);
                            failureCallback(err);
                        }
                    },

                    // ✅ Hover tooltip
                    eventMouseEnter: (info) => {
                        const ep = info.event.extendedProps || {};
                        const title = info.event.title || "Training";

                        const html = `
            <div style="font-size:13px; line-height:1.3">
              <div style="font-weight:700; margin-bottom:6px">${escapeHtml(title)}</div>
              ${ep.city ? `<div><b>City:</b> ${escapeHtml(ep.city)}</div>` : ""}
              ${ep.trainingLocation ? `<div><b>Location:</b> ${escapeHtml(ep.trainingLocation)}</div>` : ""}
              ${ep.virtualUrl
                                ? `<div style="margin-top:6px"><b>Link:</b> ${escapeHtml(ep.virtualUrl)}</div>`
                                : ""
                            }
            </div>
          `;

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

                    // ✅ Click -> open CourseDetailModal using /api/Course/{id}
                    eventClick: async (clickInfo) => {
                        clickInfo.jsEvent.preventDefault();

                        const ep = clickInfo?.event?.extendedProps || {};
                        const courseSysId = ep.courseSysId;

                        // (optional) Ctrl/⌘ + click -> open URL
                        const ctrl = clickInfo.jsEvent.ctrlKey || clickInfo.jsEvent.metaKey;
                        const url = ep.virtualUrl;
                        if (ctrl && url) {
                            window.open(url, "_blank", "noopener");
                            return;
                        }

                        if (!courseSysId) return;

                        try {
                            // close tooltip (if still open)
                            if (clickInfo.el?._tippy) {
                                clickInfo.el._tippy.destroy();
                                clickInfo.el._tippy = null;
                            }

                            // ✅ Load full course object for modal
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

                // If modal is open and user clicks Register, retry registration after login
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