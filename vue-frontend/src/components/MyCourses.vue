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
            <!-- Left-aligned Tabs -->
            <div class="tab-header">
                <button :class="{ active: activeTab === 'inProgress' }" @click="activeTab = 'inProgress'">In Progress</button>
                <button :class="{ active: activeTab === 'completed' }" @click="activeTab = 'completed'">Completed</button>
            </div>

            <div v-if="loading" class="loading">Loading your courses...</div>
            <div v-else-if="filteredCourses.length === 0" class="no-data">No {{ activeTab }} courses.</div>

            <div v-else class="course-list">
                <div v-for="course in filteredCourses"
                     :key="course.courseSysId"
                     :class="['course-card', { waitlisted: course.isWaitlisted }]"
                     role="button"
                     tabindex="0"
                     @click="openCourseDetail(course.courseSysId)">

                    <div class="course-media">
                        <div v-if="course.formatLabel" class="format-badge-outside">
                            {{ course.formatLabel }}
                        </div>

                        <div class="course-image" :style="getImageStyle()"></div>
                    </div>
                    <div class="course-details">
                        <h3 class="course-title">{{ truncateText(course.subjectTitle, 70) }}</h3>
                        <p class="course-date"><strong>Date:</strong> {{ formatDate(course.courseDate) }}</p>
                        <p class="course-time"><strong>Time:</strong> {{ truncateText(course.courseTime || 'N/A', 40) }}</p>
                        <p class="course-desc">{{ truncateText(course.subjectDescription || 'No description provided.', 120) }}</p>
                        <div v-if="course.isWaitlisted" class="waitlist-banner">
                            <span class="icon">⏳</span>
                            <span class="message">You are currently on the waitlist for this course.</span>
                        </div>
                    </div>

                    <div class="course-actions">
                        <!-- Progress ring placed above buttons -->
                        <div v-if="course.status === 1" class="progress-ring">
                            <svg viewBox="0 0 36 36">
                                <path class="bg" d="M18 2.0845a 15.9155 15.9155 0 1 1 0 31.831" />
                                <path class="progress" :stroke-dasharray="`${course.progress}, 100`" d="M18 2.0845a 15.9155 15.9155 0 1 1 0 31.831" />
                                <text x="18" y="20.35" class="percentage">{{ course.progress }}%</text>
                            </svg>
                        </div>

                        <!-- Status Badge -->
                        <div v-if="course.status === 2 || course.status === 4" class="status-tag">
                            {{ course.status === 2 ? "Cancelled" : "Absent" }}
                        </div>

                        <!-- ONLINE TRAINING (SCORM) -->
                        <button v-if="course.status === 1 && course.format === 2"
                                class="launch-btn"
                                @click.stop="launchCourse(course.courseSysId, course.scormButtonLabel)">
                            {{ course.scormButtonLabel || "Launch Course" }}
                        </button>

                        <!-- OTHER FORMATS (NON-SCORM) -->
                        <button v-if="course.status === 1 && course.format !== 2"
                                class="details-btn"
                                @click.stop="openDetails(course)">
                            View Details
                        </button>

                        <button v-if="course.status === 1"
                                class="drop-btn"
                                @click.stop="openDropConfirm(course.courseSysId)">
                            Drop
                        </button>

                        <button v-if="course.status === 3"
                                class="certificate-btn"
                                @click.stop>
                            View Certificate
                        </button>
                    </div>
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
</template>

<script>import apiClient from "@/axios";
    import DropCourseConfirmModal from "@/components/Modals/DropCourseConfirmModal.vue";
    import UserCourseViewModal from "@/components/Modals/UserCourseViewModal.vue";
    import ScormPlayer from "@/components/ScormPlayer.vue";
    import CourseDetailsModal from "@/components/Modals/CourseViewModal.vue";

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

                // SCORM player state
                isPlaying: false,
                player: null
            };
        },
        computed: {
            filteredCourses() {
    if (this.activeTab === "inProgress") {
      return this.allCourses
        .filter(c => c.status === 1)
        .map(c => ({
          ...JSON.parse(JSON.stringify(c)),
          progress: c.scormProgress ?? 0, // ✅ from API
          scormButtonLabel: c.scormButtonLabel ?? "Launch Course",
          scormCompleted: !!c.scormCompleted
        }));
    } else {
      return this.allCourses.filter(c => [2, 3, 4].includes(c.status));
    }
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

  // =========================================
  //  LOCAL (ACTIVE)
  // BASE_URL should be "/"
  // =========================================
  const base = (process.env.BASE_URL || "/").replace(/\/$/, "");
  const launchUrl = `${base}${c.videoUrl}`;

  // =========================================
  //  AIDEV (COMMENTED)
  // BASE_URL should be "/HIVTrainingDemo/"
  // If you want to hardcode instead of relying on BASE_URL:
  // const AIDEV_BASE = "http://aidev/HIVTrainingDemo";
  // const launchUrl = `${AIDEV_BASE}${c.videoUrl}`;
  // =========================================

  // ✅ Backend expects GUID (string) in InitRequest.userId
  const userId = localStorage.getItem("userId");
  if (!userId) {
    alert("userId missing. Please login again (userId not found in localStorage).");
    return;
  }

  // quick GUID format check (prevents silent 400)
  const guidRegex =
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/;
  if (!guidRegex.test(userId)) {
    alert(`userId in localStorage is not a valid GUID:\n${userId}\n\nFix: store the real Guid userId after login.`);
    return;
  }

  // ✅ IMPORTANT: your backend InitRequest has scoId as int?
  // so DO NOT send "sco-demo" string
  const scoId = 1; // choose 1 as a stable default (or use null if you don't care)

  try {
    const initRes = await apiClient.post(`/scorm/runtime/init`, {
      userId,             // ✅ GUID string
      scormId: courseId,  // ✅ int
        scoId,
        forceNewAttempt
    });

    const { registrationId, preloadCmi, scoId: serverScoId } = initRes.data;

    const courseTitle = c.subjectTitle || "Course";

    this.player = {
      launchUrl,
      registrationId,
      // use serverScoId if backend returns it, fallback to our scoId
      scoId: serverScoId ?? String(scoId),
      preloadCmi,
      title: courseTitle
    };

    this.isPlaying = true;
  } catch (err) {
    const msg = err?.response?.data || err?.message || err;
    console.error("SCORM init failed:", msg, err);

    // show helpful backend message (400/404/etc)
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
                        formatLabel: c.formatLabel ?? c.FormatLabel ?? null
                    }));                } catch (err) {
                    console.error("Error fetching user courses:", err);
                } finally {
                    this.loading = false;
                }
            },

            getImageStyle() {
                const imageUrl = require("@/assets/hiv2.png");
                return {
                    backgroundImage: `url(${imageUrl})`,
                    backgroundSize: "cover",
                    backgroundPosition: "center",
                    backgroundRepeat: "no-repeat",
                };
            },
            truncateText(text, maxLength) {
                return text.length > maxLength ? text.slice(0, maxLength) + "..." : text;
            },
            formatDate(date) {
                return new Date(date).toLocaleDateString();
            },
        },
        mounted() {
            this.fetchUserCourses();
        },
    };</script>

<style scoped>
    .my-learnings-page {
        padding: 24px;
        background-color: #f4f6f8;
        min-height: 100vh;
    }

    .player-wrap {
        min-height: 80vh;
    }

    .tab-header {
        display: flex;
        justify-content: flex-start;
        gap: 10px;
        margin-bottom: 24px;
    }

        .tab-header button {
            padding: 10px 20px;
            background-color: #d3d3d3;
            border-radius: 8px;
            font-weight: 600;
            border: none;
            cursor: pointer;
            color: #333;
            transition: 0.3s;
        }

            .tab-header button.active {
                background-color: #3f51b5;
                color: white;
            }

    .loading, .no-data {
        text-align: center;
        font-size: 1.4rem;
        color: #666;
        margin-top: 40px;
    }

    .course-list {
        display: flex;
        flex-direction: column;
        gap: 24px;
    }

    .course-card {
        display: flex;
        background-color: white;
        border-radius: 16px;
        box-shadow: 0 6px 20px rgba(0, 0, 0, 0.08);
        padding: 20px;
        align-items: flex-start;
        gap: 20px;
        cursor: pointer;
        transition: transform 0.2s ease, box-shadow 0.2s ease;
    }

        .course-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
        }

    .course-image {
        width: 180px;
        height: 120px;
        border-radius: 12px;
        background-size: cover;
        background-position: center;
        position: relative; 
        overflow: hidden;
    }
    .format-badge {
        position: absolute;
        top: 10px;
        left: 10px;
        padding: 6px 10px;
        border-radius: 999px;
        font-size: 0.78rem;
        font-weight: 700;
        background: rgba(0, 0, 0, 0.65);
        color: white;
        backdrop-filter: blur(4px);
    }

    .course-details {
        flex: 1;
    }

    .course-title {
        font-size: 1.2rem;
        font-weight: 700;
        margin-bottom: 8px;
    }

    .course-date, .course-time, .course-desc {
        font-size: 0.95rem;
        color: #555;
        margin-bottom: 6px;
    }

    .course-actions {
        display: flex;
        flex-direction: column;
        gap: 10px;
        align-items: center;
        justify-content: center;
        min-width: 140px;
    }

    /* add details-btn here */
    .launch-btn, .drop-btn, .certificate-btn, .details-btn {
        padding: 8px 16px;
        border-radius: 20px;
        border: none;
        font-weight: bold;
        font-size: 0.9rem;
        cursor: pointer;
        color: white;
        transition: background-color 0.2s ease;
    }

    .launch-btn {
        background-color: #4caf50;
    }

        .launch-btn:hover {
            background-color: #388e3c;
        }

    .drop-btn {
        background-color: #f44336;
    }

        .drop-btn:hover {
            background-color: #c62828;
        }

    .certificate-btn {
        background-color: #3f51b5;
    }

        .certificate-btn:hover {
            background-color: #2c3e9f;
        }

    .progress-ring {
        width: 60px;
        height: 60px;
        margin-bottom: 10px;
    }

        .progress-ring svg {
            transform: rotate(-3600deg);
            width: 100%;
            height: 100%;
        }

        .progress-ring .bg {
            fill: none;
            stroke: #eee;
            stroke-width: 3.8;
        }

        .progress-ring .progress {
            fill: none;
            stroke: #3f51b5;
            stroke-width: 3.8;
            stroke-linecap: round;
            transition: stroke-dasharray 0.5s ease;
        }

        .progress-ring .percentage {
            fill: #3f51b5;
            font-size: 10px;
            text-anchor: middle;
            dominant-baseline: middle;
        }

    .status-tag {
        background-color: #ffdddd;
        color: #d32f2f;
        padding: 6px 14px;
        border-radius: 14px;
        font-weight: 600;
        font-size: 0.85rem;
        margin-top: 6px;
        text-align: center;
    }

    .course-card.waitlisted {
        opacity: 0.5;
        pointer-events: none;
    }

    .waitlist-banner {
        display: flex;
        align-items: center;
        background-color: #fff3cd;
        border-left: 5px solid #ff9800;
        padding: 10px 14px;
        border-radius: 8px;
        font-size: 0.95rem;
        font-weight: 500;
        color: #5d4037;
        margin-top: 12px;
        box-shadow: 0 2px 6px rgba(255, 152, 0, 0.2);
    }

        .waitlist-banner .icon {
            font-size: 1.2rem;
            margin-right: 8px;
            color: #ff9800;
        }

        .waitlist-banner .message {
            flex: 1;
        }

    @media (max-width: 768px) {
        .my-learnings-page {
            padding: 12px;
        }

        .course-card {
            flex-direction: column;
        }

        .course-image {
            width: 100%;
            height: 160px;
        }
    }
    .course-media {
        display: flex;
        flex-direction: column;
        gap: 8px;
        width: 180px; /* match image width */
    }

    .format-badge-outside {
        align-self: flex-start;
        padding: 6px 10px;
        border-radius: 999px;
        font-size: 0.78rem;
        font-weight: 700;
        background: rgba(0,0,0,0.65);
        color: #fff;
    }
    .details-btn {
        background-color: #607d8b; /* blue-grey */
    }

        .details-btn:hover {
            background-color: #455a64;
        }
</style>