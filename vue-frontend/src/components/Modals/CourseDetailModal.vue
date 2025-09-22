<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-content">
            <!-- Title Banner -->
            <div class="modal-banner" :style="bannerStyle">
                <h2 class="course-title">{{ course.subjectTitle }}</h2>
                <div class="banner-actions">
                    <template v-if="alreadyRegistered">
                        <span class="registered-pill">✅ Registered</span>
                    </template>
                    <template v-else>
                        <button class="btn-primary btn-register-top" @click.stop="registerCourse">
                            Register
                        </button>
                    </template>

                    <button class="close-btn" @click="$emit('close')">&times;</button>
                </div>
            </div>

            <div class="modal-body">
                <div class="background-logo"></div>
                <!-- Multi-session Note -->
                <div v-if="course.isMultiSession" class="multi-session-note">
                    ⚠️ <strong>Note:</strong> This course consists of multiple sessions. You must attend all sessions to receive a certificate.
                </div>

                <!-- Course Details -->
                <div class="modal-section">
                    <h4 class="section-title">📋 Course Details</h4>

                    <div class="grid grid-top">
                        <div><strong>Date:</strong> <span class="value">{{ formatDate(course.courseDate) }}</span></div>
                        <div>
                            <strong>Time:</strong>
                            <span class="time-text"
                                  :class="{ clamped: !timeExpanded }"
                                  :title="course.courseTime">
                                {{ course.courseTime || 'N/A' }}
                            </span>
                            <button v-if="needsTimeClamp"
                                    class="moreless"
                                    @click="timeExpanded = !timeExpanded">
                                {{ timeExpanded ? 'less' : 'more…' }}
                            </button>
                        </div>
                        <div><strong>Location:</strong> <span class="value">{{ course.trainingLocation || 'N/A' }}</span></div>
                        <div><strong>Training Center:</strong> <span class="value">{{ course.siteName || 'N/A' }}</span></div>
                        <div><strong>Format:</strong> <span class="value">{{ course.formatLabel?.trim() || 'N/A' }}</span></div>
                    </div>

                    <!-- Description section moved here -->
                    <div class="modal-section description-inline">
                        <h4 class="section-title">📝 Description</h4>
                        <div v-html="course.subjectDescription || 'No description available.'"
                             class="rich-html-content">
                        </div>
                    </div>

                    <!-- Second line (remaining fields) -->
                    <div class="grid grid-bottom">
                        <div><strong>Category:</strong> <span class="value">{{ course.categoryLabel?.trim() || 'N/A' }}</span></div>
                        <div><strong>Region:</strong> <span class="value">{{ course.regionLabel?.trim() || 'N/A' }}</span></div>
                        <div><strong>CNE Credits:</strong> <span class="value">{{ course.cnecredits ? 'Yes' : 'No' }}</span></div>
                        <div><strong>OASAS Credits:</strong> <span class="value">{{ course.oasascredits ? 'Yes' : 'No' }}</span></div>
                        <div><strong>Peer Cert Hours:</strong> <span class="value">{{ course.peerCertCredits ? 'Yes' : 'No' }}</span></div>
                        <div><strong>Credit Hours:</strong> <span class="value">{{ course.creditHrs || 'N/A' }}</span></div>
                    </div>
                </div>

                <!-- Description -->
                <!--<div class="modal-section">
        <h4 class="section-title">📝 Description</h4>
        <div v-html="course.subjectDescription || 'No description available.'" class="rich-html-content"></div>
    </div>-->
                <!-- Presenter Section -->
                <div class="modal-section" v-if="hasPresenters">
                    <h4 class="section-title">👨‍🏫 Presenter</h4>
                    <div v-if="course.instructorLabel" class="presenter-block">
                        <div @click="toggleNote('instructor1')" class="presenter-name">
                            <strong>{{ course.instructorLabel }}</strong>
                            <span class="dropdown-icon">{{ showNote1 ? '▲' : '▼' }}</span>
                        </div>
                        <div v-if="showNote1 && course.instructorNote" class="presenter-note">
                            {{ course.instructorNote }}
                        </div>
                    </div>
                    <div v-if="course.instructor2Label" class="presenter-block">
                        <div @click="toggleNote('instructor2')" class="presenter-name">
                            <strong>{{ course.instructor2Label }}</strong>
                            <span class="dropdown-icon">{{ showNote2 ? '▲' : '▼' }}</span>
                        </div>
                        <div v-if="showNote2 && course.instructor2Note" class="presenter-note">
                            {{ course.instructor2Note }}
                        </div>
                    </div>
                </div>

                <!-- Session Schedule -->
                <div v-if="course.sessions?.$values?.length" class="modal-section">
                    <h4 class="section-title">📅 Schedule</h4>
                    <div class="session-block" v-for="(session, index) in course.sessions.$values" :key="index">
                        <div><strong>Session {{ index + 1 }}</strong></div>
                        <div><strong>Date:</strong> {{ formatDate(session.date) }}</div>
                        <div><strong>Start Time:</strong> {{ formatTime(session.startTime) }}</div>
                        <div><strong>End Time:</strong> {{ formatTime(session.endTime) }}</div>
                    </div>
                </div>

                <!-- ADA Requirement Section -->
                <div class="modal-section">
                    <label class="ada-checkbox">
                        <input type="checkbox"
                               v-model="adaNeeded"
                               @change="handleAdaCheckbox" />
                        Will you require special accommodation under the Americans with Disability Act (ADA) to participate in trainings?
                    </label>

                    <!-- ADA Confirmation Prompt -->
                    <div v-if="adaNeeded && showAdaConfirm" class="ada-confirm-box">
                        <p><strong>Are you sure you require ADA accommodations?</strong></p>
                        <div class="button-group">
                            <button @click="confirmAda">Yes, I need ADA</button>
                            <button @click="cancelAda">No, I do not</button>
                        </div>
                    </div>

                    <!-- ADA Details Textbox -->
                    <div v-if="adaNeeded" class="ada-details-box">
                        <label for="adaDetails"><strong>Please describe the accommodation:</strong></label>
                        <textarea v-model="adaDetails"
                                  id="adaDetails"
                                  placeholder="Enter accommodation details..."
                                  @keydown.stop></textarea>
                    </div>
                </div>

                <!-- Buttons -->
                <div class="button-group">
                    <template v-if="alreadyRegistered">
                        <span style="color: #388e3c; font-weight: bold;">✅ This course is already registered.</span>
                        <button class="btn-secondary" @click="$emit('close')">Close</button>
                    </template>
                    <template v-else>
                        <button class="btn-primary" @click="registerCourse">Register</button>
                        <button class="btn-secondary" @click="$emit('close')">Cancel</button>
                    </template>
                </div>
            </div>
        </div>
    </div>
</template>

<script>import imagew from '@/assets/img.png';
import apiClient from "@/axios";

export default {
    props: ["course"],
    data() {
        return {
            showNote1: false,
            showNote2: false,
            alreadyRegistered: false,
            // ADA
            adaNeeded: false,
            adaDetails: "",
            showAdaConfirm: false,     // only for first manual “turn on” flow
            adaPrefilled: false,       // <-- flag so we don’t show confirm when prefilled

            timeExpanded: false,
        };
    },
    computed: {
        hasPresenters() {
            return this.course.instructorLabel || this.course.instructor2Label;
        }, needsTimeClamp() {
            return (this.course?.courseTime?.length || 0) > 80;
        },
       
        bannerStyle() {
            return {
                backgroundImage: `url(${imagew})`,
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                backgroundRepeat: 'no-repeat'
            };
        }
    },
        async mounted() {
            document.addEventListener("keydown", this.handleKeydown);

            try {
                const userId = localStorage.getItem("userId");
                const res = await apiClient.get(`/Course/check-registered`, {
                    params: {
                        userId,
                        courseId: this.course.courseSysId
                    }
                });

                this.alreadyRegistered = !!res.data?.isRegistered;

                // 1) If registered and we have course-level ADA, use that.
                if (this.alreadyRegistered && res.data?.courseAda) {
                    this.adaNeeded = !!res.data.courseAda.adaneed;
                    this.adaDetails = res.data.courseAda.adadetails || "";
                    this.adaPrefilled = this.adaNeeded;
                    this.showAdaConfirm = false;
                    return;
                }

                // 2) Otherwise, fall back to user profile ADA.
                const ada = res.data?.userAda;
                if (ada) {
                    this.adaNeeded = !!ada.adaneed;
                    this.adaDetails = ada.adadetails || "";
                    this.adaPrefilled = this.adaNeeded;
                    this.showAdaConfirm = false;
                }
            } catch (err) {
                console.error("Registration check failed:", err);
            }
        },
    unmounted() {
        document.removeEventListener("keydown", this.handleKeydown);
    },
    methods: {
        handleLoginSuccess(userData) {
        localStorage.setItem("userId", userData.userId);
        localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
        localStorage.setItem("jwtToken", userData.token);

        this.showLoginModal = false;

        // ✅ Retry registration if a course was selected
        if (this.selectedCourse) {
            this.handleRegister(this.selectedCourse);
        }
    },
        registerCourse() {
            const userId = localStorage.getItem("userId");
            if (!userId) {
                this.$emit("request-login");
                return;
            }

            this.$emit("register", {
                ...this.course,
                adaneed: this.adaNeeded,
                adadetails: this.adaNeeded ? (this.adaDetails || null) : null
            });
        },
        handleAdaCheckbox() {
            // If this state came from the server (prefilled), don’t show confirm
            if (this.adaPrefilled) {
                this.showAdaConfirm = false;
                this.adaPrefilled = false; // only skip once; subsequent manual toggles work normally
                return;
            }

            // User is toggling manually now
            if (this.adaNeeded) {
                // show confirm only when turning ON
                this.showAdaConfirm = true;
            } else {
                // turning OFF clears details & confirm
                this.showAdaConfirm = false;
                this.adaDetails = "";
            }
        },
        confirmAda() {
            this.showAdaConfirm = false;
        },
        cancelAda() {
            // user said “No”, revert toggle
            this.adaNeeded = false;
            this.adaDetails = "";
            this.showAdaConfirm = false;
        },
        handleKeydown(e) {
            // Don't react to keys typed inside form fields/contenteditable
            const tag = (e.target?.tagName || "").toLowerCase();
            const isEditable =
                tag === "input" ||
                tag === "textarea" ||
                tag === "select" ||
                e.target?.isContentEditable;

            if (isEditable) return;

            // Close on Escape
            if (e.key === "Escape") {
                this.$emit("close");
                return;
            }

            // Optional: only treat Ctrl/⌘ + K as a shortcut (but don't close the modal)
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
                e.preventDefault();
                // do nothing (or open search, etc.)
            }
        },
        toggleNote(instructor) {
            if (instructor === 'instructor1') this.showNote1 = !this.showNote1;
            if (instructor === 'instructor2') this.showNote2 = !this.showNote2;
        },
        formatDate(date) {
            return new Date(date).toLocaleDateString();
        },
        formatTime(time) {
            if (!time) return "N/A";
            const [hours, minutes] = time.split(":");
            const date = new Date();
            date.setHours(hours, minutes);
            return date.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
        },
    },
};</script>
<style scoped>
    /* ===== Overlay & Shell ===== */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal-content {
        background: #fff;
        width: 75vw;
        max-height: 90vh;
        overflow-y: auto;
        border-radius: 12px;
        box-shadow: 0 8px 20px rgba(0,0,0,.15);
        font-family: 'Segoe UI', sans-serif;
        font-size: 16.5px;
    }

    /* ===== Banner ===== */
    .modal-banner {
        background-color: #f1f3f6;
        padding: 20px 24px;
        border-top-left-radius: 12px;
        border-top-right-radius: 12px;
        border-bottom: 1px solid #ddd;
        display: flex;
        align-items: center;
        gap: 12px;
        min-height: 120px;
    }

    .course-title {
        font-size: 22px;
        font-weight: 600;
        color: #ebeff2;
        margin: 0;
        flex: 1 1 auto;
    }

    .banner-actions {
        display: flex;
        align-items: center;
        gap: 10px;
        margin-left: auto;
    }

    .btn-register-top {
        padding: 8px 14px;
        font-size: 15px;
        border-radius: 6px;
    }

    .registered-pill {
        background: rgba(56,142,60,.1);
        color: #2e7d32;
        border: 1px solid rgba(46,125,50,.25);
        padding: 6px 10px;
        border-radius: 9999px;
        font-weight: 600;
        font-size: 14px;
    }

    .close-btn {
        color: #fff;
        background: rgba(0,0,0,.35);
        border: 1px solid rgba(255,255,255,.7);
        border-radius: 9999px;
        width: 40px;
        height: 40px;
        font-size: 26px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        transition: background .15s ease, transform .08s ease;
        box-shadow: 0 2px 8px rgba(0,0,0,.25);
    }

        .close-btn:hover {
            background: rgba(0,0,0,.5);
            transform: translateY(-1px);
        }

        .close-btn:focus {
            outline: 2px solid #fff;
            outline-offset: 2px;
        }

    @media (hover:none) {
        .close-btn:hover {
            transform: none;
        }
    }

    /* ===== Body ===== */
    .modal-body {
        position: relative;
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 24px;
        z-index: 1;
    }

        .modal-body::before {
            content: "";
            background-image: url('@/assets/imagewht.png');
            background-repeat: no-repeat;
            background-position: center;
            background-size: 300px auto;
            opacity: .05;
            position: absolute;
            inset: 0;
            z-index: 0;
            pointer-events: none;
        }

    /* ===== Sections ===== */
    .section-title {
        font-size: 18px;
        font-weight: 600;
        color: #333;
        border-bottom: 1px solid #ddd;
        margin-bottom: 12px;
        padding-bottom: 4px;
    }

    .multi-session-note {
        background: #fff8e1;
        border-left: 4px solid #ffc107;
        padding: 12px 16px;
        border-radius: 6px;
        font-size: 16px;
        color: #5d4037;
    }

    .session-block {
        background: #fafafa;
        border: 1px solid #e0e0e0;
        padding: 12px 14px;
        border-radius: 6px;
        font-size: 16px;
    }

    /* ===== Grids ===== */
    .grid {
        display: grid;
        gap: 16px;
    }

    .grid-top {
        grid-template-columns: repeat(5, minmax(180px, 1fr));
        column-gap: 16px;
        row-gap: 12px;
    }

    .grid-bottom {
        grid-template-columns: repeat(4, minmax(180px, 1fr));
        column-gap: 16px;
        row-gap: 12px;
    }

        /* Tight label/value rows – removes the huge gap */
        .grid-top > div,
        .grid-bottom > div {
            display: flex;
            align-items: baseline;
            gap: 8px;
            flex-wrap: nowrap;
            min-width: 0;
        }

            .grid-top > div strong,
            .grid-bottom > div strong {
                margin: 0;
                white-space: nowrap;
                font-weight: 700;
            }

    .value {
        min-width: 0;
        overflow-wrap: anywhere;
        word-break: break-word;
    }

    /* Responsive */
    @media (max-width:1280px) {
        .grid-top {
            grid-template-columns: repeat(4, minmax(160px,1fr));
        }
    }

    @media (max-width:1024px) {
        .grid-top {
            grid-template-columns: repeat(3, minmax(160px,1fr));
        }

        .grid-bottom {
            grid-template-columns: repeat(3, minmax(160px,1fr));
        }
    }

    @media (max-width:768px) {
        .grid-top {
            grid-template-columns: repeat(2, minmax(150px,1fr));
        }

        .grid-bottom {
            grid-template-columns: repeat(2, minmax(150px,1fr));
        }
    }

    @media (max-width:520px) {
        .grid-top, .grid-bottom {
            grid-template-columns: 1fr;
        }
    }

    /* ===== Presenter ===== */
    .presenter-name {
        background: #f5f5f5;
        padding: 10px 14px;
        border-radius: 6px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        cursor: pointer;
        font-weight: 500;
        color: #333;
        font-size: 16px;
    }

    .presenter-note {
        background: #fefefe;
        padding: 10px 14px;
        margin-top: 6px;
        margin-left: 8px;
        border-left: 4px solid #ccc;
        border-radius: 6px;
        font-style: italic;
        font-size: 15px;
        color: #555;
    }

    /* ===== Buttons ===== */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 12px;
    }

    .btn-primary, .btn-secondary {
        padding: 10px 20px;
        font-size: 16px;
        border-radius: 6px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color .2s ease;
    }

    .btn-primary {
        background: #388e3c;
        color: #fff;
        border: none;
    }

        .btn-primary:hover {
            background: #2e7d32;
        }

    .btn-secondary {
        background: #e0e0e0;
        color: #333;
        border: none;
    }

        .btn-secondary:hover {
            background: #d0d0d0;
        }

    /* ===== Description (rich HTML) ===== */
    .description-inline {
        margin: 12px 0;
    }

    .rich-html-content {
        font-size: 15.5px;
        line-height: 1.7;
        color: #333;
    }

        .rich-html-content ul {
            padding-left: 20px;
            margin-bottom: 16px;
        }

        .rich-html-content li {
            margin-bottom: 8px;
        }

        .rich-html-content a {
            color: #1976d2;
            text-decoration: underline;
        }

        .rich-html-content strong {
            font-weight: bold;
        }

    /* ===== ADA ===== */
    .ada-checkbox {
        display: flex;
        align-items: center;
        gap: 10px;
        font-size: 15px;
        margin-bottom: 10px;
    }

    .ada-confirm-box {
        background: #fff3cd;
        border-left: 4px solid #ffc107;
        padding: 12px;
        border-radius: 6px;
        margin-top: 8px;
    }

        .ada-confirm-box .button-group {
            justify-content: flex-start;
        }

            .ada-confirm-box .button-group button:first-child {
                background: #1976d2;
                color: #fff;
            }

                .ada-confirm-box .button-group button:first-child:hover {
                    background: #1565c0;
                }

            .ada-confirm-box .button-group button:last-child {
                background: #eee;
                color: #333;
            }

                .ada-confirm-box .button-group button:last-child:hover {
                    background: #d6d6d6;
                }

    .ada-details-box {
        margin-top: 12px;
    }

        .ada-details-box textarea {
            width: 100%;
            min-height: 80px;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 15px;
            resize: vertical;
        }
    .time-text {
        display: -webkit-box;
        -webkit-box-orient: vertical;
        overflow: hidden;
        overflow-wrap: anywhere;
        word-break: break-word;
        line-height: 1.5;
    }

        .time-text.clamped {
            -webkit-line-clamp: 2; /* ✅ show only 2 lines */
            max-height: calc(1.5em * 2); /* fallback for some browsers */
        }

    .moreless {
        background: none;
        border: none;
        padding: 0;
        margin-left: 6px;
        font: inherit;
        color: #1976d2;
        cursor: pointer;
        text-decoration: underline;
        white-space: nowrap;
    }

        .moreless:hover {
            text-decoration: none;
        }
    /* === ADA (stronger, local-only) === */
    .modal-content .ada-checkbox {
        display: flex;
        align-items: center;
        gap: 10px;
        font-size: 15px;
        margin-bottom: 10px;
    }

    .modal-content .ada-confirm-box {
        background: #fff3cd;
        border-left: 4px solid #ffc107;
        padding: 12px;
        border-radius: 6px;
        margin-top: 8px;
    }

        /* make ADA confirm buttons left-aligned and spaced, regardless of the global .button-group */
        .modal-content .ada-confirm-box .button-group {
            display: flex;
            gap: 12px;
            margin-top: 10px;
            justify-content: flex-start;
        }

            .modal-content .ada-confirm-box .button-group button {
                padding: 10px 20px;
                font-size: 15px;
                font-weight: 600;
                border: none;
                border-radius: 6px;
                cursor: pointer;
                transition: background-color .3s ease;
            }

                .modal-content .ada-confirm-box .button-group button:first-child {
                    background: #1976d2;
                    color: #fff;
                }

                    .modal-content .ada-confirm-box .button-group button:first-child:hover {
                        background: #1565c0;
                    }

                .modal-content .ada-confirm-box .button-group button:last-child {
                    background: #eee;
                    color: #333;
                }

                    .modal-content .ada-confirm-box .button-group button:last-child:hover {
                        background: #d6d6d6;
                    }

    .modal-content .ada-details-box {
        margin-top: 12px;
    }

        .modal-content .ada-details-box textarea {
            width: 100%;
            min-height: 80px;
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 6px;
            font-size: 15px;
            resize: vertical;
        }
</style>