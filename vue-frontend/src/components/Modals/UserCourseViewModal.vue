<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-content">
            <!-- Title Banner -->
            <div class="modal-banner" >
                <h2 class="course-title">{{ course.subjectTitle }}</h2>
                <button class="close-btn" @click="$emit('close')">&times;</button>
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
                    <div class="grid">
                        <div><strong>Date:</strong> {{ formatDate(course.courseDate) }}</div>
                        <div><strong>Time:</strong> {{ course.courseTime || 'N/A' }}</div>
                        <div><strong>Location:</strong> {{ course.trainingLocation || 'N/A' }}</div>
                        <div><strong>Training Center:</strong> {{ course.siteName || 'N/A' }}</div>
                        <div><strong>Format:</strong> {{ course.formatLabel?.trim() || 'N/A' }}</div>
                        <div><strong>Category:</strong> {{ course.categoryLabel?.trim() || 'N/A' }}</div>
                        <div><strong>Region:</strong> {{ course.regionLabel?.trim() || 'N/A' }}</div>
                        <div><strong>CNE Credits:</strong> {{ course.cnecredits ? 'Yes' : 'No' }}</div>
                        <div><strong>OASAS Credits:</strong> {{ course.oasascredits ? 'Yes' : 'No' }}</div>
                        <div><strong>Peer Cert Hours:</strong> {{ course.peerCertCredits ? 'Yes' : 'No' }}</div>
                        <div><strong>Credit Hours:</strong> {{ course.creditHrs || 'N/A' }}</div>
                    </div>
                </div>

                <!-- Description -->
                <div class="modal-section">
                    <h4 class="section-title">🗘️ Description</h4>
                    <div v-html="course.subjectDescription || 'No description available.'" class="rich-html-content"></div>
                </div>

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
                    <h4 class="section-title">🗓️ Schedule</h4>
                    <div class="session-block" v-for="(session, index) in course.sessions.$values" :key="index">
                        <div><strong>Session {{ index + 1 }}</strong></div>
                        <div><strong>Date:</strong> {{ formatDate(session.date) }}</div>
                        <div><strong>Start Time:</strong> {{ formatTime(session.startTime) }}</div>
                        <div><strong>End Time:</strong> {{ formatTime(session.endTime) }}</div>
                    </div>
                </div>

                <!-- Buttons -->
                <div class="button-group">
                    <button v-if="(activeTab === 'inProgress' && course.status === 1) || (activeTab === 'completed' && course.status === 3)"
                            class="launch-btn"
                            @click.stop="launchCourse(course.courseSysId)">
                        Launch Course
                    </button>

                    <button class="btn-secondary" @click="$emit('drop-course', course.courseSysId, true)">
                        Drop Course
                    </button>
                    <button class="btn-secondary" @click="$emit('close')">Close</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>

    export default {
        props: ["course"],
        data() {
            return {
                showNote1: false,
                showNote2: false,
            };
        },
        computed: {
            hasPresenters() {
                return this.course.instructorLabel || this.course.instructor2Label;
            },
            
        },
        methods: {
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
        }
    };</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0, 0, 0, 0.6);
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
        box-shadow: 0 8px 20px rgba(0, 0, 0, 0.15);
        font-family: 'Segoe UI', sans-serif;
        font-size: 16.5px;
    }

    .modal-banner {
        background: #43285D;
        color: white;
        padding: 28px 40px;
        border-top-left-radius: 12px;
        border-top-right-radius: 12px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        min-height: 110px;
    }

    .course-title {
        font-size: 26px;
        font-weight: 700;
        color: #ffffff;
        margin: 0;
        word-break: break-word;
        max-width: 85%;
        white-space: normal;
        line-height: 1.35;
    }

    .close-btn {
        color: #ffffff;
        background: rgba(0, 0, 0, 0.35);
        border: 1px solid rgba(255, 255, 255, 0.7);
        border-radius: 999px;
        width: 40px;
        height: 40px;
        font-size: 26px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
    }

    .modal-body {
        position: relative;
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 24px;
        z-index: 1; /* Ensure content is on top */
    }

        .modal-body::before {
            content: "";
            background-image: url('@/assets/imagewht.png'); /* Use correct path */
            background-repeat: no-repeat;
            background-position: center;
            background-size: 300px auto;
            opacity: 0.05; /* 👈 very light */
            position: absolute;
            top: 0;
            bottom: 0;
            left: 0;
            right: 0;
            z-index: 0;
            pointer-events: none; /* Makes it click-through */
        }

    .section-title {
        font-size: 18px;
        font-weight: 600;
        color: #333;
        border-bottom: 1px solid #ddd;
        margin-bottom: 12px;
        padding-bottom: 4px;
    }

    .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 16px;
    }

    .multi-session-note {
        background-color: #fff8e1;
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

    .presenter-name {
        background-color: #f5f5f5;
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
        background-color: #fefefe;
        padding: 10px 14px;
        margin-top: 6px;
        margin-left: 8px;
        border-left: 4px solid #ccc;
        border-radius: 6px;
        font-style: italic;
        font-size: 15px;
        color: #555;
    }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 12px;
    }

    .btn-primary,
    .btn-secondary {
        padding: 10px 20px;
        font-size: 16px;
        border-radius: 6px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

    .btn-primary {
        background-color: #388e3c;
        color: white;
        border: none;
    }

        .btn-primary:hover {
            background-color: #2e7d32;
        }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        border: none;
    }

        .btn-secondary:hover {
            background-color: #d0d0d0;
        }

    .modal-banner {
        background-size: cover;
        background-position: center;
        background-repeat: no-repeat;
        padding: 20px 24px;
        border-top-left-radius: 12px;
        border-top-right-radius: 12px;
        border-bottom: 1px solid #ddd;
        display: flex;
        justify-content: space-between;
        align-items: center;
        min-height: 120px; /* Ensures visibility of image */
    }

    .modal-body {
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 24px;
        background-image: url('@/assets/imagewht.png');
        background-repeat: no-repeat;
        background-position: center 80px;
        background-size: 300px auto;
        opacity: 1;
    }

    .modal-body {
        padding: 24px;
        display: flex;
        flex-direction: column;
        gap: 24px;
        position: relative;
        z-index: 1;
    }

    .background-logo {
        content: "";
        position: absolute;
        top: 100px;
        left: 50%;
        transform: translateX(-50%);
        width: 300px;
        height: 300px;
        background-image: url('@/assets/imagewht.png');
        background-repeat: no-repeat;
        background-size: contain;
        background-position: center;
        opacity: 0.07;
        pointer-events: none;
        z-index: 0;
    }

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

    .ada-confirm-box .button-group {
        display: flex;
        gap: 12px;
        margin-top: 10px;
        justify-content: flex-start;
    }

        .ada-confirm-box .button-group button {
            padding: 10px 20px;
            font-size: 15px;
            font-weight: 600;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            transition: background-color 0.3s ease;
        }

            .ada-confirm-box .button-group button:first-child {
                background-color: #1976d2; /* Primary Blue */
                color: white;
            }

                .ada-confirm-box .button-group button:first-child:hover {
                    background-color: #1565c0;
                }

            .ada-confirm-box .button-group button:last-child {
                background-color: #eeeeee; /* Light Gray */
                color: #333;
            }

                .ada-confirm-box .button-group button:last-child:hover {
                    background-color: #d6d6d6;
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
</style>