<template>
    <div class="my-learnings-page">
        <!-- PLAYER MODE -->
        <div v-if="isPlaying" class="player-wrap">
            <ScormPlayer :launchUrl="player.launchUrl"
                         :registrationId="player.registrationId"
                         :scoId="player.scoId"
                         :preloadCmi="player.preloadCmi"
                         :title="player.title"
                         @exit="exitPlayer" />
        </div>

        <!-- LIST MODE -->
        <div v-else>
            <div class="page-hero">
                <div class="page-hero-left">
                    <span class="page-chip">MY LEARNINGS</span>
                    <h1 class="page-title">My Learning Dashboard</h1>
                    <p class="page-subtitle">
                        Track your active, attended, absent, cancelled, and dropped courses in one place.
                    </p>
                </div>
            </div>

            <div class="tab-header">
                <button :class="{ active: activeTab === 'inProgress' }" @click="activeTab = 'inProgress'">In Progress</button>
                <button :class="{ active: activeTab === 'attended' }" @click="activeTab = 'attended'">Attended</button>
                <button :class="{ active: activeTab === 'absent' }" @click="activeTab = 'absent'">Absent</button>
                <button :class="{ active: activeTab === 'cancelled' }" @click="activeTab = 'cancelled'">Cancelled</button>
                <button :class="{ active: activeTab === 'dropped' }" @click="activeTab = 'dropped'">Dropped</button>
            </div>

            <div v-if="loading" class="loading-wrap">
                <div class="loading-card">Loading your courses...</div>
            </div>

            <div v-else-if="filteredCourses.length === 0" class="empty-state">
                <div class="empty-icon">📘</div>
                <h3>
                    No {{
                    activeTab === 'inProgress'
                        ? 'in progress'
                        : activeTab === 'attended'
                            ? 'attended'
                            : activeTab === 'dropped'
                                ? 'dropped'
                                : activeTab
                    }} courses
                </h3>
                <p>Courses in this section will appear here once available.</p>
            </div>

            <div v-else class="course-list">
                <div v-for="course in filteredCourses"
                     :key="course.courseSysId"
                     :class="['course-card', `card-${course.learningSection}`, { waitlisted: course.isWaitlisted }]"
                     role="button"
                     tabindex="0"
                     @click="openCourseDetail(course.courseSysId)">
                    <div class="course-media">
                        <div v-if="course.formatLabel" class="format-badge-outside">
                            {{ course.formatLabel }}
                        </div>

                        <div class="course-image" :style="getImageStyle(course)">
                            <div class="image-overlay"></div>
                        </div>
                    </div>

                    <div class="course-content">
                        <div class="course-top-row">
                            <div class="course-header-block">
                                <h3 class="course-title">
                                    {{ truncateText(course.subjectTitle || 'Untitled Course', 85) }}
                                </h3>

                                <div class="course-meta">
                                    <span class="meta-pill">
                                        <strong>Date:</strong> {{ formatDate(course.courseDate) }}
                                    </span>
                                    <span class="meta-pill">
                                        <strong>Time:</strong> {{ truncateText(course.courseTime || 'N/A', 40) }}
                                    </span>
                                </div>
                            </div>

                            <div v-if="course.learningSection !== 'inProgress'"
                                 :class="['status-tag', course.learningSection]">
                                {{
                                    course.learningSection === 'cancelled' ? 'Cancelled' :
                                    course.learningSection === 'absent' ? 'Absent' :
                                    course.learningSection === 'attended' ? 'Attended' :
                                    course.learningSection === 'dropped' ? 'Dropped' :
                                    ''
                                }}
                            </div>
                        </div>

                        <p class="course-desc">
                            {{ truncateText(stripHtml(course.subjectDescription || 'No description provided.'), 160) }}
                        </p>

                        <div v-if="course.isWaitlisted" class="waitlist-banner">
                            <span class="icon">⏳</span>
                            <span class="message">You are currently on the waitlist for this course.</span>
                        </div>
                    </div>

                    <div class="course-actions">
                        <div v-if="course.learningSection === 'inProgress' && course.format === 2" class="progress-ring-wrap">
                            <div class="progress-ring">
                                <svg viewBox="0 0 36 36">
                                    <path class="bg" d="M18 2.0845a 15.9155 15.9155 0 1 1 0 31.831" />
                                    <path class="progress"
                                          :stroke-dasharray="`${course.progress}, 100`"
                                          d="M18 2.0845a 15.9155 15.9155 0 1 1 0 31.831" />
                                    <text x="18" y="20.35" class="percentage">{{ course.progress }}%</text>
                                </svg>
                            </div>
                            <span class="progress-label">Progress</span>
                        </div>

                        <button v-if="course.learningSection === 'inProgress' && course.format === 2"
                                class="launch-btn"
                                @click.stop="launchCourse(course.courseSysId, course.scormButtonLabel)">
                            {{ course.scormButtonLabel || "Launch Course" }}
                        </button>

                        <button v-if="course.learningSection === 'inProgress' && course.format !== 2"
                                class="details-btn"
                                @click.stop="openDetails(course)">
                            View Details
                        </button>

                        <button v-if="course.learningSection === 'inProgress'"
                                class="drop-btn"
                                @click.stop="openDropConfirm(course.courseSysId)">
                            Drop
                        </button>

                        <button v-if="course.learningSection === 'attended'"
                                class="certificate-btn"
                                @click.stop="openCertificate(course)">
                            View Certificate
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <DropCourseConfirmModal v-if="showDropModal"
                                @close="showDropModal = false"
                                @confirm-drop="confirmDrop" />

        <UserCourseViewModal v-if="selectedUserCourse"
                             :course="selectedUserCourse"
                             @close="selectedUserCourse = null"
                             @launch-course="launchCourse"
                             @drop-course="openDropConfirm" />

        <CourseDetailsModal v-if="showDetailsModal"
                            :course="detailsCourse"
                            @close="closeDetails" />

        <div v-if="showCertificateModal" class="certificate-modal-overlay" @click.self="closeCertificateModal">
            <div class="certificate-modal">
                <div class="certificate-modal-header">
                    <div>
                        <h3>Certificate Preview</h3>
                        <p>{{ certificateCourseTitle }}</p>
                    </div>

                    <div class="certificate-toolbar">
                        <button class="toolbar-btn download" @click="downloadCertificate">Download</button>
                        <button class="toolbar-btn print" @click="printCertificate">Print</button>
                        <button class="toolbar-btn close" @click="closeCertificateModal">Close</button>
                    </div>
                </div>

                <div class="certificate-frame-wrap">
                    <iframe v-if="certificateUrl"
                            :src="certificateUrl"
                            class="certificate-frame"
                            title="Certificate Preview">
                    </iframe>
                </div>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";
    import DropCourseConfirmModal from "@/components/Modals/DropCourseConfirmModal.vue";
    import UserCourseViewModal from "@/components/Modals/UserCourseViewModal.vue";
    import ScormPlayer from "@/components/ScormPlayer.vue";
    import CourseDetailsModal from "@/components/Modals/CourseViewModal.vue";

    import img1 from "@/assets/images/img1.jpeg";
    import img2 from "@/assets/images/img2.jpeg";
    import img3 from "@/assets/images/img3.jpeg";
    import img4 from "@/assets/images/img4.jpeg";
    import img5 from "@/assets/images/img5.jpeg";

    export default {
        components: {
            DropCourseConfirmModal,
            UserCourseViewModal,
            ScormPlayer,
            CourseDetailsModal
        },
        name: "MyLearningsPage",
        data() {
            return {
                activeTab: "inProgress",
                loading: false,
                allCourses: [],
                showDropModal: false,
                selectedCourseId: null,
                selectedUserCourse: null,
                closeUserCourseModalAfterDrop: false,
                showDetailsModal: false,
                detailsCourse: null,
                isPlaying: false,
                player: null,
                showCertificateModal: false,
                certificateUrl: null,
                certificateCourseTitle: "",
                defaultCourseImages: [img1, img2, img3, img4, img5]
            };
        },
        computed: {
            filteredCourses() {
                return this.allCourses.filter(c => c.learningSection === this.activeTab);
            }
        },
        methods: {
            openDetails(course) {
                this.detailsCourse = course;
                this.showDetailsModal = true;
            },

            closeDetails() {
                this.showDetailsModal = false;
                this.detailsCourse = null;
            },

            openCertificate(course) {
                const userId = localStorage.getItem("userId");
                if (!userId || !course?.courseSysId) return;

                const timestamp = new Date().getTime();
                this.certificateUrl = `/api/Course/certificate/${course.courseSysId}?userId=${userId}&t=${timestamp}`;
                this.certificateCourseTitle = course.subjectTitle || "Certificate";
                this.showCertificateModal = true;
            },

            closeCertificateModal() {
                this.showCertificateModal = false;
                this.certificateUrl = null;
                this.certificateCourseTitle = "";
            },

            downloadCertificate() {
                if (!this.certificateUrl) return;

                const downloadUrl = this.certificateUrl.includes("?")
                    ? `${this.certificateUrl}&download=true`
                    : `${this.certificateUrl}?download=true`;

                window.location.href = downloadUrl;
            },

            printCertificate() {
                if (!this.certificateUrl) return;

                const printWindow = window.open(this.certificateUrl, "_blank");
                if (printWindow) {
                    printWindow.focus();
                }
            },

            stripHtml(html) {
                if (!html) return "";
                const div = document.createElement("div");
                div.innerHTML = html;
                return (div.textContent || div.innerText || "").trim();
            },

            async launchCourse(courseId, label = "Launch Course") {
                const forceNewAttempt = (label || "").toLowerCase().includes("retake");
                const c = this.allCourses.find(x => x.courseSysId === courseId);
                if (!c) return;

                if (c.format !== 2) {
                    alert("This course is not an Online Training (SCORM).");
                    return;
                }

                if (!c.videoUrl) {
                    alert("SCORM URL (VideoURL) is missing for this training.");
                    return;
                }

                const base = (process.env.BASE_URL || "/").replace(/\/$/, "");
                const launchUrl = `${base}${c.videoUrl}`;

                const userId = localStorage.getItem("userId");
                if (!userId) {
                    alert("userId missing. Please login again (userId not found in localStorage).");
                    return;
                }

                const guidRegex =
                    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/;

                if (!guidRegex.test(userId)) {
                    alert(`userId in localStorage is not a valid GUID:\n${userId}\n\nFix: store the real Guid userId after login.`);
                    return;
                }

                const scoId = 1;

                try {
                    const initRes = await apiClient.post(`/scorm/runtime/init`, {
                        userId,
                        scormId: courseId,
                        scoId,
                        forceNewAttempt
                    });

                    const { registrationId, preloadCmi, scoId: serverScoId } = initRes.data;
                    const courseTitle = c.subjectTitle || "Course";

                    this.player = {
                        launchUrl,
                        registrationId,
                        scoId: serverScoId ?? String(scoId),
                        preloadCmi,
                        title: courseTitle
                    };

                    this.isPlaying = true;
                } catch (err) {
                    const msg = err?.response?.data || err?.message || err;
                    console.error("SCORM init failed:", msg, err);
                    alert(typeof msg === "string" ? msg : "Unable to start SCORM session. Please try again.");
                }
            },

            exitPlayer() {
                this.isPlaying = false;
                this.player = null;
                this.fetchUserCourses();
            },

            openDropConfirm(courseId, closeUserModal = false) {
                this.selectedCourseId = courseId;
                this.showDropModal = true;
                if (closeUserModal) this.closeUserCourseModalAfterDrop = true;
            },

            async dropCourse(courseId) {
                const userId = localStorage.getItem("userId");
                if (!userId) return;

                try {
                    await apiClient.post(`/Course/drop`, { userId, courseId });
                    this.fetchUserCourses();
                } catch (err) {
                    console.error("Failed to drop course:", err);
                }
            },

            async openCourseDetail(courseId) {
                try {
                    const res = await apiClient.get(`/Course/${courseId}`);
                    if (res.data) this.selectedUserCourse = res.data;
                } catch (err) {
                    console.error("Failed to load full course detail:", err);
                }
            },

            async confirmDrop() {
                const userId = localStorage.getItem("userId");
                if (!userId || !this.selectedCourseId) return;

                try {
                    await apiClient.post(`/Course/drop`, { userId, courseId: this.selectedCourseId });
                    this.showDropModal = false;

                    if (this.closeUserCourseModalAfterDrop) {
                        this.selectedUserCourse = null;
                        this.closeUserCourseModalAfterDrop = false;
                    }

                    this.fetchUserCourses();
                } catch (err) {
                    console.error("Failed to drop course:", err);
                }
            },

            async fetchUserCourses() {
                const userId = localStorage.getItem("userId");
                if (!userId) return;

                this.loading = true;
                try {
                    const res = await apiClient.get(`/Course/user-courses/${userId}`);
                    this.allCourses = (res.data?.$values || res.data || []).map(c => ({
                        ...c,
                        formatLabel: c.formatLabel ?? c.FormatLabel ?? null,
                        titleImageUrl: c.titleImageUrl ?? c.TitleImageUrl ?? null,
                        learningSection: c.learningSection ?? c.LearningSection ?? "inProgress",
                        progress: c.scormProgress ?? c.ScormProgress ?? 0,
                        scormButtonLabel: c.scormButtonLabel ?? c.ScormButtonLabel ?? "Launch Course",
                        scormCompleted: c.scormCompleted ?? c.ScormCompleted ?? false,
                        attended: c.attended ?? c.Attended ?? false
                    }));
                } catch (err) {
                    console.error("Error fetching user courses:", err);
                } finally {
                    this.loading = false;
                }
            },

            getImageStyle(course) {
                let imageUrl = course?.titleImageUrl;

                if (!imageUrl) {
                    const index = Math.abs(Number(course?.courseSysId || 0)) % this.defaultCourseImages.length;
                    imageUrl = this.defaultCourseImages[index];
                }

                return {
                    backgroundImage: `url("${imageUrl}")`,
                    backgroundSize: "cover",
                    backgroundPosition: "center",
                    backgroundRepeat: "no-repeat"
                };
            },

            truncateText(text, maxLength) {
                const safeText = text || "";
                return safeText.length > maxLength ? safeText.slice(0, maxLength) + "..." : safeText;
            },

            formatDate(date) {
                if (!date) return "N/A";
                return new Date(date).toLocaleDateString();
            }
        },

        mounted() {
            this.fetchUserCourses();
        }
    };</script>

<style scoped>
    .my-learnings-page {
        padding: 28px;
        background: linear-gradient(180deg, #f6f7fb 0%, #eef2f7 100%);
        min-height: 100vh;
    }

    .player-wrap {
        min-height: 80vh;
    }

    .page-hero {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        margin-bottom: 22px;
    }

    .page-chip {
        display: inline-block;
        padding: 7px 14px;
        border-radius: 999px;
        background: #ece7f6;
        color: #6b4ea2;
        font-size: 0.78rem;
        font-weight: 800;
        letter-spacing: 0.04em;
        margin-bottom: 12px;
    }

    .page-title {
        font-size: 2rem;
        font-weight: 800;
        color: #1f2937;
        margin: 0 0 8px;
        line-height: 1.2;
    }

    .page-subtitle {
        margin: 0;
        font-size: 1rem;
        color: #5f6b7a;
        line-height: 1.6;
        max-width: 760px;
    }

    .tab-header {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
        margin-bottom: 24px;
    }

        .tab-header button {
            padding: 11px 20px;
            background: #e5e7eb;
            border-radius: 14px;
            font-weight: 700;
            border: none;
            cursor: pointer;
            color: #374151;
            transition: all 0.25s ease;
            box-shadow: 0 2px 6px rgba(15, 23, 42, 0.04);
        }

            .tab-header button:hover {
                transform: translateY(-1px);
                background: #dbe2ea;
            }

            .tab-header button.active {
                background: linear-gradient(135deg, #4c63d2, #3953c5);
                color: white;
                box-shadow: 0 10px 24px rgba(63, 81, 181, 0.22);
            }

    .loading-wrap {
        display: flex;
        justify-content: center;
        margin-top: 32px;
    }

    .loading-card {
        padding: 18px 24px;
        border-radius: 16px;
        background: white;
        color: #556070;
        font-size: 1rem;
        box-shadow: 0 10px 28px rgba(15, 23, 42, 0.08);
    }

    .empty-state {
        background: white;
        border-radius: 24px;
        padding: 54px 24px;
        text-align: center;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06);
        border: 1px solid #edf1f5;
    }

    .empty-icon {
        font-size: 2.5rem;
        margin-bottom: 12px;
    }

    .empty-state h3 {
        font-size: 1.35rem;
        color: #1f2937;
        margin-bottom: 8px;
    }

    .empty-state p {
        color: #667085;
        margin: 0;
    }

    .course-list {
        display: flex;
        flex-direction: column;
        gap: 22px;
    }

    .course-card {
        display: grid;
        grid-template-columns: 210px 1fr 180px;
        gap: 22px;
        background: rgba(255, 255, 255, 0.96);
        border-radius: 26px;
        padding: 22px;
        align-items: stretch;
        cursor: pointer;
        border: 1px solid #edf1f5;
        box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
        transition: transform 0.22s ease, box-shadow 0.22s ease;
    }

        .course-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 18px 38px rgba(15, 23, 42, 0.11);
        }

    .course-media {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .format-badge-outside {
        align-self: flex-start;
        padding: 7px 14px;
        border-radius: 999px;
        font-size: 0.78rem;
        font-weight: 800;
        background: linear-gradient(135deg, #4b5563, #6b7280);
        color: #fff;
        box-shadow: 0 8px 18px rgba(75, 85, 99, 0.18);
    }

    .course-image {
        width: 100%;
        height: 145px;
        border-radius: 18px;
        overflow: hidden;
        position: relative;
        box-shadow: inset 0 0 0 1px rgba(255,255,255,0.18);
    }

    .image-overlay {
        position: absolute;
        inset: 0;
        background: linear-gradient(180deg, rgba(17,24,39,0.05) 0%, rgba(17,24,39,0.18) 100%);
    }

    .course-content {
        display: flex;
        flex-direction: column;
        justify-content: center;
        min-width: 0;
    }

    .course-top-row {
        display: flex;
        justify-content: space-between;
        gap: 16px;
        align-items: flex-start;
        margin-bottom: 12px;
    }

    .course-header-block {
        min-width: 0;
    }

    .course-title {
        font-size: 1.55rem;
        font-weight: 800;
        color: #172033;
        margin: 0 0 14px;
        line-height: 1.28;
        word-break: break-word;
    }

    .course-meta {
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
    }

    .meta-pill {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 8px 12px;
        border-radius: 999px;
        background: #f5f7fb;
        border: 1px solid #e7ebf2;
        color: #475467;
        font-size: 0.92rem;
    }

    .course-desc {
        font-size: 1rem;
        color: #5b6472;
        line-height: 1.7;
        margin: 4px 0 0;
        max-width: 95%;
    }

    .course-actions {
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: flex-end;
        gap: 12px;
    }

    .progress-ring-wrap {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 4px;
        margin-bottom: 6px;
    }

    .progress-label {
        font-size: 0.8rem;
        color: #72809a;
        font-weight: 700;
    }

    .progress-ring {
        width: 72px;
        height: 72px;
    }

        .progress-ring svg {
            width: 100%;
            height: 100%;
            transform: rotate(-90deg);
        }

        .progress-ring .bg {
            fill: none;
            stroke: #e9edf5;
            stroke-width: 3.4;
        }

        .progress-ring .progress {
            fill: none;
            stroke: #5568d8;
            stroke-width: 3.4;
            stroke-linecap: round;
            transition: stroke-dasharray 0.45s ease;
        }

        .progress-ring .percentage {
            fill: #4c63d2;
            font-size: 9px;
            font-weight: 800;
            text-anchor: middle;
            dominant-baseline: middle;
            transform: rotate(90deg);
            transform-origin: center;
        }

    .launch-btn,
    .drop-btn,
    .certificate-btn,
    .details-btn {
        min-width: 145px;
        padding: 11px 18px;
        border-radius: 999px;
        border: none;
        font-weight: 800;
        font-size: 0.95rem;
        cursor: pointer;
        color: white;
        transition: all 0.2s ease;
        box-shadow: 0 8px 18px rgba(15, 23, 42, 0.08);
    }

    .launch-btn {
        background: linear-gradient(135deg, #63c266, #44a847);
    }

        .launch-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 12px 22px rgba(68, 168, 71, 0.24);
        }

    .drop-btn {
        background: linear-gradient(135deg, #ff6b5f, #ef4444);
    }

        .drop-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 12px 22px rgba(239, 68, 68, 0.24);
        }

    .certificate-btn {
        background: linear-gradient(135deg, #5b6fe8, #3f51b5);
    }

        .certificate-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 12px 22px rgba(63, 81, 181, 0.24);
        }

    .details-btn {
        background: linear-gradient(135deg, #728196, #58687c);
    }

        .details-btn:hover {
            transform: translateY(-1px);
            box-shadow: 0 12px 22px rgba(88, 104, 124, 0.22);
        }

    .status-tag {
        padding: 8px 16px;
        border-radius: 999px;
        font-weight: 800;
        font-size: 0.85rem;
        text-align: center;
        white-space: nowrap;
        box-shadow: inset 0 0 0 1px rgba(255,255,255,0.2);
    }

        .status-tag.absent {
            background: #fff4e5;
            color: #b26a00;
        }

        .status-tag.cancelled {
            background: #ffebee;
            color: #c62828;
        }

        .status-tag.attended {
            background: #e8f5e9;
            color: #2e7d32;
        }

        .status-tag.dropped {
            background: #f3e5f5;
            color: #7b1fa2;
        }

    .waitlist-banner {
        display: flex;
        align-items: center;
        gap: 10px;
        background: linear-gradient(135deg, #fff8e8, #fff4d8);
        border-left: 5px solid #f5b25d;
        padding: 12px 15px;
        border-radius: 14px;
        font-size: 0.96rem;
        font-weight: 600;
        color: #7a5b22;
        margin-top: 16px;
    }

        .waitlist-banner .icon {
            font-size: 1.1rem;
        }

    .course-card.waitlisted {
        opacity: 1;
        pointer-events: auto;
    }

    .card-attended {
        border-left: 6px solid #58b368;
    }

    .card-absent {
        border-left: 6px solid #d9a441;
    }

    .card-cancelled {
        border-left: 6px solid #e57373;
    }

    .card-dropped {
        border-left: 6px solid #9c6ade;
    }

    .card-inProgress {
        border-left: 6px solid #5c6fd9;
    }

    @media (max-width: 1100px) {
        .course-card {
            grid-template-columns: 200px 1fr;
        }

        .course-actions {
            grid-column: 1 / -1;
            flex-direction: row;
            flex-wrap: wrap;
            justify-content: flex-start;
            align-items: center;
            margin-top: 6px;
        }

        .progress-ring-wrap {
            margin-right: 10px;
        }
    }

    @media (max-width: 768px) {
        .my-learnings-page {
            padding: 16px;
        }

        .page-title {
            font-size: 1.6rem;
        }

        .course-card {
            grid-template-columns: 1fr;
            padding: 18px;
        }

        .course-image {
            height: 180px;
        }

        .course-top-row {
            flex-direction: column;
            align-items: flex-start;
        }

        .course-actions {
            align-items: stretch;
        }

        .launch-btn,
        .drop-btn,
        .certificate-btn,
        .details-btn {
            width: 100%;
        }

        .course-desc {
            max-width: 100%;
        }
    }
    .certificate-modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.55);
        backdrop-filter: blur(4px);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 3000;
        padding: 24px;
    }

    .certificate-modal {
        width: min(1200px, 96vw);
        height: min(92vh, 900px);
        background: #ffffff;
        border-radius: 24px;
        overflow: hidden;
        box-shadow: 0 30px 80px rgba(15, 23, 42, 0.28);
        display: flex;
        flex-direction: column;
    }

    .certificate-modal-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 16px;
        padding: 18px 22px;
        border-bottom: 1px solid #e8edf4;
        background: linear-gradient(180deg, #fbfcfe 0%, #f5f7fb 100%);
    }

        .certificate-modal-header h3 {
            margin: 0;
            font-size: 1.2rem;
            font-weight: 800;
            color: #172033;
        }

        .certificate-modal-header p {
            margin: 4px 0 0;
            color: #667085;
            font-size: 0.92rem;
        }

    .certificate-toolbar {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
    }

    .toolbar-btn {
        border: none;
        border-radius: 999px;
        padding: 10px 16px;
        font-weight: 700;
        cursor: pointer;
        color: white;
        transition: all 0.2s ease;
    }

        .toolbar-btn.download {
            background: linear-gradient(135deg, #4c63d2, #3953c5);
        }

        .toolbar-btn.print {
            background: linear-gradient(135deg, #44a847, #63c266);
        }

        .toolbar-btn.close {
            background: linear-gradient(135deg, #7b8794, #5f6c7b);
        }

    .certificate-frame-wrap {
        flex: 1;
        background: #eef2f7;
    }

    .certificate-frame {
        width: 100%;
        height: 100%;
        border: none;
        background: white;
    }
</style>