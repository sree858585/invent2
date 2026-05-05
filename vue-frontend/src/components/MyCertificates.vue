<template>
    <div class="certificates-page">
        <div class="page-hero">
            <span class="page-chip">MY CERTIFICATES</span>
            <h1 class="page-title">My Certificates</h1>
            <p class="page-subtitle">
                View, download, and print certificates for successfully completed courses.
            </p>
        </div>

        <div v-if="loading" class="loading-card">
            Loading your certificates...
        </div>

        <div v-else-if="certificateCourses.length === 0" class="empty-state">
            <div class="empty-icon">📜</div>
            <h3>No certificates available</h3>
            <p>Your completed course certificates will appear here.</p>
        </div>

        <div v-else class="certificate-list">
            <div v-for="course in certificateCourses"
                 :key="course.courseSysId"
                 class="certificate-card">
                <div class="course-media">
                    <div v-if="course.formatLabel" class="format-badge">
                        {{ course.formatLabel }}
                    </div>

                    <div class="course-image" :style="getImageStyle(course)">
                        <div class="image-overlay"></div>
                    </div>
                </div>

                <div class="certificate-content">
                    <div class="status-row">
                        <span class="status-tag">Completed</span>
                    </div>

                    <h3 class="course-title">
                        {{ truncateText(course.subjectTitle || "Untitled Course", 90) }}
                    </h3>

                    <div class="course-meta">
                        <span class="meta-pill">
                            <strong>Completed:</strong> {{ formatDate(course.endDate || course.courseDate) }}
                        </span>
                        <span class="meta-pill">
                            <strong>Time:</strong> {{ truncateText(course.courseTime || "N/A", 45) }}
                        </span>
                    </div>

                    <p class="course-desc">
                        {{ truncateText(stripHtml(course.subjectDescription || "No description provided."), 170) }}
                    </p>
                </div>

                <div class="certificate-actions">
                    <button class="view-btn" @click="openCertificate(course)">
                        View Certificate
                    </button>

                    <button class="download-btn" @click="downloadCertificateDirect(course)">
                        Download
                    </button>

                    <button class="print-btn" @click="printCertificateDirect(course)">
                        Print
                    </button>
                </div>
            </div>
        </div>

        <div v-if="showCertificateModal"
             class="certificate-modal-overlay"
             @click.self="closeCertificateModal">
            <div class="certificate-modal">
                <div class="certificate-modal-header">
                    <div>
                        <h3>Certificate Preview</h3>
                        <p>{{ certificateCourseTitle }}</p>
                    </div>

                    <div class="certificate-toolbar">
                        <button class="toolbar-btn download" @click="downloadCertificate">
                            Download
                        </button>
                        <button class="toolbar-btn print" @click="printCertificate">
                            Print
                        </button>
                        <button class="toolbar-btn close" @click="closeCertificateModal">
                            Close
                        </button>
                    </div>
                </div>

                <div class="certificate-frame-wrap">
                    <iframe v-if="certificateUrl"
                            :src="certificateUrl"
                            class="certificate-frame"
                            title="Certificate Preview"></iframe>
                </div>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios";

    import img1 from "@/assets/images/img1.jpg";
    import img2 from "@/assets/images/img2.jpg";
    import img3 from "@/assets/images/img3.jpg";
    import img4 from "@/assets/images/img4.jpg";
    import img5 from "@/assets/images/img5.jpg";

    export default {
        name: "MyCertificates",

        data() {
            return {
                loading: false,
                allCourses: [],
                showCertificateModal: false,
                certificateUrl: null,
                certificateCourseTitle: "",
                defaultCourseImages: [img1, img2, img3, img4, img5],
            };
        },

      
            computed: {
    certificateCourses() {
        return this.allCourses.filter(c =>
            c.learningSection === "attended" &&
            c.status !== 6 &&
            c.status !== 2 &&
            c.cancelled !== true
        );
    },
},

        methods: {
            async fetchUserCourses() {
                const userId = localStorage.getItem("userId");
                if (!userId) {
                    this.$router.push("/home");
                    return;
                }

                this.loading = true;

                try {
                    const res = await apiClient.get(`/Course/user-courses/${userId}`);

                    this.allCourses = (res.data?.$values || res.data || []).map((c) => ({
    ...c,
    courseSysId: c.courseSysId ?? c.CourseSysId,
    status: c.status ?? c.Status,
    attended: c.attended ?? c.Attended ?? false,
    cancelled: c.cancelled ?? c.Cancelled ?? false,
    formatLabel: c.formatLabel ?? c.FormatLabel ?? null,
    titleImageUrl: c.titleImageUrl ?? c.TitleImageUrl ?? null,
    learningSection: c.learningSection ?? c.LearningSection ?? "inProgress",
    scormCompleted: c.scormCompleted ?? c.ScormCompleted ?? false,
    subjectTitle: c.subjectTitle ?? c.SubjectTitle ?? null,
    subjectDescription: c.subjectDescription ?? c.SubjectDescription ?? null,
    courseDate: c.courseDate ?? c.CourseDate ?? null,
    endDate: c.endDate ?? c.EndDate ?? null,
    courseTime: c.courseTime ?? c.CourseTime ?? null,
}));
                } catch (err) {
                    console.error("Error fetching certificates:", err);
                } finally {
                    this.loading = false;
                }
            },

            buildCertificateUrl(course, download = false) {
                const userId = localStorage.getItem("userId");
                const timestamp = new Date().getTime();

                let url = `/api/Course/certificate/${course.courseSysId}?userId=${userId}&t=${timestamp}`;

                if (download) {
                    url += "&download=true";
                }

                return url;
            },

            openCertificate(course) {
                if (!course?.courseSysId) return;

                this.certificateUrl = this.buildCertificateUrl(course, false);
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
                if (printWindow) printWindow.focus();
            },

            downloadCertificateDirect(course) {
                window.location.href = this.buildCertificateUrl(course, true);
            },

            printCertificateDirect(course) {
                const printWindow = window.open(this.buildCertificateUrl(course, false), "_blank");
                if (printWindow) printWindow.focus();
            },

            getImageStyle(course) {
                let imageUrl = course?.titleImageUrl;

                if (!imageUrl) {
                    const index =
                        Math.abs(Number(course?.courseSysId || 0)) % this.defaultCourseImages.length;
                    imageUrl = this.defaultCourseImages[index];
                }

                return {
                    backgroundImage: `url("${imageUrl}")`,
                    backgroundSize: "cover",
                    backgroundPosition: "center",
                    backgroundRepeat: "no-repeat",
                };
            },

            stripHtml(html) {
                if (!html) return "";
                const div = document.createElement("div");
                div.innerHTML = html;
                return (div.textContent || div.innerText || "").trim();
            },

            truncateText(text, maxLength) {
                const safeText = text || "";
                return safeText.length > maxLength
                    ? safeText.slice(0, maxLength) + "..."
                    : safeText;
            },

            formatDate(date) {
                if (!date) return "N/A";
                return new Date(date).toLocaleDateString();
            },
        },

        mounted() {
            this.fetchUserCourses();
        },
    };</script>

<style scoped>
    .certificates-page {
        padding: 28px;
        background: linear-gradient(180deg, #f6f7fb 0%, #eef2f7 100%);
        min-height: 100vh;
    }

    .page-hero {
        margin-bottom: 24px;
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
    }

    .page-subtitle {
        margin: 0;
        font-size: 1rem;
        color: #5f6b7a;
        line-height: 1.6;
    }

    .loading-card,
    .empty-state {
        background: #ffffff;
        border-radius: 24px;
        padding: 42px 24px;
        text-align: center;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06);
        border: 1px solid #edf1f5;
        color: #556070;
    }

    .empty-icon {
        font-size: 2.7rem;
        margin-bottom: 12px;
    }

    .empty-state h3 {
        color: #1f2937;
        margin-bottom: 8px;
    }

    .certificate-list {
        display: flex;
        flex-direction: column;
        gap: 22px;
    }

    .certificate-card {
        display: grid;
        grid-template-columns: 210px 1fr 210px;
        gap: 22px;
        background: rgba(255, 255, 255, 0.96);
        border-radius: 26px;
        padding: 22px;
        align-items: stretch;
        border: 1px solid #edf1f5;
        border-left: 6px solid #58b368;
        box-shadow: 0 14px 34px rgba(15, 23, 42, 0.07);
        transition: transform 0.22s ease, box-shadow 0.22s ease;
    }

        .certificate-card:hover {
            transform: translateY(-3px);
            box-shadow: 0 18px 38px rgba(15, 23, 42, 0.11);
        }

    .course-media {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .format-badge {
        align-self: flex-start;
        padding: 7px 14px;
        border-radius: 999px;
        font-size: 0.78rem;
        font-weight: 800;
        background: linear-gradient(135deg, #4b5563, #6b7280);
        color: #fff;
    }

    .course-image {
        width: 100%;
        height: 145px;
        border-radius: 18px;
        overflow: hidden;
        position: relative;
    }

    .image-overlay {
        position: absolute;
        inset: 0;
        background: linear-gradient(180deg, rgba(17, 24, 39, 0.05) 0%, rgba(17, 24, 39, 0.18) 100%);
    }

    .certificate-content {
        display: flex;
        flex-direction: column;
        justify-content: center;
    }

    .status-row {
        margin-bottom: 10px;
    }

    .status-tag {
        padding: 8px 16px;
        border-radius: 999px;
        font-weight: 800;
        font-size: 0.85rem;
        background: #e8f5e9;
        color: #2e7d32;
    }

    .course-title {
        font-size: 1.5rem;
        font-weight: 800;
        color: #172033;
        margin: 0 0 14px;
        line-height: 1.3;
    }

    .course-meta {
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
    }

    .meta-pill {
        display: inline-flex;
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
        margin: 14px 0 0;
    }

    .certificate-actions {
        display: flex;
        flex-direction: column;
        justify-content: center;
        align-items: flex-end;
        gap: 12px;
    }

    .view-btn,
    .download-btn,
    .print-btn {
        min-width: 165px;
        padding: 11px 18px;
        border-radius: 999px;
        border: none;
        font-weight: 800;
        font-size: 0.95rem;
        cursor: pointer;
        color: white;
        transition: all 0.2s ease;
    }

    .view-btn {
        background: linear-gradient(135deg, #5b6fe8, #3f51b5);
    }

    .download-btn {
        background: linear-gradient(135deg, #44a847, #63c266);
    }

    .print-btn {
        background: linear-gradient(135deg, #728196, #58687c);
    }

        .view-btn:hover,
        .download-btn:hover,
        .print-btn:hover {
            transform: translateY(-1px);
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

    @media (max-width: 1100px) {
        .certificate-card {
            grid-template-columns: 200px 1fr;
        }

        .certificate-actions {
            grid-column: 1 / -1;
            flex-direction: row;
            justify-content: flex-start;
            align-items: center;
            flex-wrap: wrap;
        }
    }

    @media (max-width: 768px) {
        .certificates-page {
            padding: 16px;
        }

        .certificate-card {
            grid-template-columns: 1fr;
            padding: 18px;
        }

        .course-image {
            height: 180px;
        }

        .certificate-actions {
            align-items: stretch;
        }

        .view-btn,
        .download-btn,
        .print-btn {
            width: 100%;
        }
    }
</style>