<template>
    <div class="modal-overlay">
        <div class="modal fade-in">

            <!-- ❌ Close Button -->
            <button class="close-btn" @click="$emit('close')" aria-label="Close">&times;</button>

            <!-- HEADER (matching Schedule Course modal, using #43285D) -->
            <div class="modal-header-purple">
                <div class="header-left">
                    <h2>Edit Course</h2>
                    <p>Modify details, schedule, delivery format, and sessions.</p>
                </div>
                <span class="modal-badge">Admin • Course Editor</span>
            </div>

            <form @submit.prevent="submitUpdate">

                <!-- SECTION: Course Details -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Course Details</h3>
                            <p>Update training center, region, category and course title.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <div class="form-column">
                            <div class="form-group">
                                <label>Training Center *</label>
                                <select v-model="form.trainingCenter" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="center in lookupData.trainingCenters"
                                            :key="center.siteSysId"
                                            :value="String(center.siteSysId)">
                                        {{ center.siteName }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label>Region *</label>
                                <select v-model="form.region" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="region in lookupData.regions"
                                            :key="region.code"
                                            :value="String(region.code)">
                                        {{ region.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label>Topics <span class="required">*</span></label>

                                <div class="topic-multi" :class="{ 'required-border': topicError }">
                                    <label v-for="t in (lookupData.topics || [])" :key="t.code" class="topic-item">
                                        <input type="checkbox" :value="String(t.code)" v-model="form.topicCodes" />
                                        <span>{{ t.value }}</span>
                                    </label>
                                </div>

                                <small v-if="topicError" class="error-text">Please select at least one topic.</small>
                                <small class="hint" v-else-if="(form.topicCodes || []).length">
                                    Selected: {{ (form.topicCodes || []).length }}
                                </small>
                            </div>
                        </div>

                        <div class="form-column">
                            <div class="form-group">
                                <label>Course Title *</label>
                                <select v-model="form.courseTitle" :disabled="!(form.topicCodes || []).length" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="subject in (filteredSubjects || [])"
                                            :key="subject.subjectSysId"
                                            :value="String(subject.subjectSysId)">
                                        {{ subject.courseTitle }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label>Registration Deadline *</label>
                                <input type="date" v-model="form.regDeadline" required />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Schedule -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Schedule</h3>
                            <p>Edit start/end date and timing of the course.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <div class="form-column">
                            <div class="form-group">
                                <label>Start Date *</label>
                                <input type="date" v-model="form.startDate" required />
                            </div>

                            <div class="form-group">
                                <label>End Date *</label>
                                <input type="date" v-model="form.endDate" required />
                            </div>
                            <div class="form-group">
                                <label>Mark as New Until</label>
                                <input type="date" v-model="form.markAsNewUntil" />
                            </div>
                        </div>

                        <div class="form-column">
                            <div class="form-group inline-group">
                                <div class="inline-field">
                                    <label>Start Time *</label>
                                    <input type="time" v-model="form.startTime" required />
                                </div>

                                <div class="inline-field">
                                    <label>End Time *</label>
                                    <input type="time" v-model="form.endTime" required />
                                </div>
                                <div class="form-group">
                                    <label>Training Hours</label>
                                    <input type="number" v-model="form.baseHours" readonly />
                                </div>
                            </div>

                            <div class="form-group">
                                <label>Max Seats *</label>
                                <input type="number" min="1" v-model="form.maxSeats" required />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Format -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Format & Delivery</h3>
                            <p>Update training location or webinar link.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <div class="form-column">
                            <div class="form-group">
                                <label>Format *</label>
                                <select v-model="form.format" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="format in lookupData.formats"
                                            :key="format.code"
                                            :value="String(format.code)">
                                        {{ format.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group" v-if="form.format == 1">
                                <label>Training Center Location *</label>
                                <input type="text" v-model="form.trainingLocation" required />
                            </div>
                        </div>

                        <div class="form-column">
                            <div class="form-group" v-if="[3,4,5,6].includes(Number(form.format))">
                                <label>Webinar / Webcast URL *</label>
                                <input type="text" v-model="form.virtualUrl" required />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Multi-session -->
                <section class="section-card">
                    <div class="section-header section-header-inline">
                        <div>
                            <h3>Multi-day / Multi-session</h3>
                            <p>Modify sessions for multi-day courses.</p>
                        </div>

                        <label class="toggle-label">
                            <input type="checkbox" v-model="form.isMultiSession" />
                            <span>Enable multi-session schedule</span>
                        </label>
                    </div>

                    <div v-if="form.isMultiSession" class="form-column-full">
                        <div v-for="(session, index) in form.sessions"
                             :key="index"
                             class="session-group">
                            <div class="form-group">
                                <label>Session {{ index + 1 }} Date</label>
                                <input type="date" v-model="session.date" required />
                            </div>
                            <div class="form-group">
                                <label>Start Time</label>
                                <input type="time" v-model="session.startTime" required />
                            </div>
                            <div class="form-group">
                                <label>End Time</label>
                                <input type="time" v-model="session.endTime" required />
                            </div>
                            <div class="form-group">
                                <label>Session URL</label>
                                <input type="url" v-model="session.url" />
                            </div>
                            <div class="form-group">
                                <label>Training Location</label>
                                <input type="text" v-model="session.trainingLocation" />
                            </div>

                            <div class="session-actions" v-if="form.sessions.length > 1">
                                <button class="btn-secondary btn-danger"
                                        type="button"
                                        @click="removeSession(index)">
                                    ❌ Remove Session
                                </button>
                            </div>

                            <div class="session-actions"
                                 v-if="index === form.sessions.length - 1 && form.sessions.length < 4">
                                <button class="btn-secondary" type="button" @click="addSession">
                                    ➕ Add Session
                                </button>
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Notes -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Visibility & Notes</h3>
                            <p>Control visibility and add schedule notes.</p>
                        </div>
                    </div>

                    <div class="form-column-full">
                        <div class="checkbox-row">
                            <input type="checkbox" v-model="form.hideCourse" />
                            <label>Hide Course from public listing</label>
                        </div>

                        <div class="form-group">
                            <label>Additional Notes</label>
                            <textarea v-model="form.courseSchedule"></textarea>
                        </div>
                    </div>
                </section>

                <!-- Buttons -->
                <div class="button-group">
                    <button class="btn-secondary" type="button" @click="$emit('close')">
                        Cancel
                    </button>
                    <button class="btn-primary" type="submit">
                        Update Course
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>
<script>import apiClient from "@/axios.js";

export default {
  props: ["course"],
  data() {
    return {
      originalCourse: null,
      topicError: false,
      form: {
        trainingCenter: "",
        region: "",
        topicCodes: [],
        courseTitle: "",
        instructor1: "",
        instructor2: "",
        startDate: "",
        endDate: "",
        startTime: "",
        endTime: "",
        regDeadline: "",
        maxSeats: "",
        trainingLocation: "",
        virtualUrl: "",
        deliverables: "",
        format: "",
        fundingType: "",
        hideCourse: false,
        courseSchedule: "",
        markAsNewUntil: "",
        baseHours: "",
        isMultiSession: false,
        sessions: []
      },
      lookupData: {
        trainingCenters: [],
        regions: [],
        topics: [],
        instructors: [],
        deliverables: [],
        formats: []
      },
      filteredSubjects: []
    };
  },

  computed: {
    calculatedBaseHours() {
      // MULTI-SESSION: sum each session
      if (this.form.isMultiSession && (this.form.sessions || []).length > 0) {
        let total = 0;
        (this.form.sessions || []).forEach((s) => {
          if (s.date && s.startTime && s.endTime) {
            const start = new Date(`${s.date}T${s.startTime}:00`);
            const end = new Date(`${s.date}T${s.endTime}:00`);
            const diff = (end - start) / (1000 * 60 * 60);
            if (diff > 0) total += diff;
          }
        });
        return Number.isNaN(total) ? 0 : Number(total.toFixed(2));
      }

      // SINGLE-BLOCK MODE
      if (!this.form.startTime || !this.form.endTime) return 0;

      const start = new Date(`2000-01-01T${this.form.startTime}:00`);
      const end = new Date(`2000-01-01T${this.form.endTime}:00`);
      let diff = (end - start) / (1000 * 60 * 60);
      if (diff < 0) diff = 0;
      return Number(diff.toFixed(2));
    }
  },

  watch: {
    "form.topicCodes": {
      deep: true,
      handler(newTopics) {
        if (Array.isArray(newTopics) && newTopics.length) {
          this.fetchSubjectsByTopics(newTopics);
        } else {
          this.filteredSubjects = [];
          this.form.courseTitle = "";
        }
      }
    },
    "form.courseTitle": {
    immediate: false,
    async handler(newVal) {
      if (!newVal) return;
      if (this.loadingTopics) return;

      await this.loadTopicsForSubject(Number(newVal));
    }
  },

    "form.isMultiSession"(val) {
      if (val && (this.form.sessions || []).length === 0) {
        this.addSession();
      }
      if (!val) {
        this.form.sessions = [
          { date: "", startTime: "", endTime: "", url: "", trainingLocation: "" }
        ];
      }
    }
  },

  async mounted() {
    await this.fetchLookupData();

    const courseId = this.course.courseSysId;
    try {
      const res = await apiClient.get(`/CourseAdmin/courseWithSessions/${courseId}`);
      const courseWithSessions = res.data;
      this.originalCourse = courseWithSessions;
      await this.populateForm(courseWithSessions);
    } catch (err) {
      console.error("❌ Failed to load full course details", err);
      alert("Failed to load course details.");
    }
  },

  methods: {
    addSession() {
      this.form.sessions = this.form.sessions || [];
      if (this.form.sessions.length < 4) {
        this.form.sessions.push({
          date: "",
          startTime: "",
          endTime: "",
          url: "",
          trainingLocation: ""
        });
      }
    },
    async loadTopicsForSubject(subjectSysId) {
  try {
    this.loadingTopics = true;

    const tRes = await apiClient.get(`/CreateCourse/topicsBySubject/${subjectSysId}`);
    const codes = tRes.data?.$values || tRes.data || [];

    // checkbox values are strings -> keep strings
    this.form.topicCodes = (codes || []).map(x => String(x));

    // refresh title list based on these topics
    if ((this.form.topicCodes || []).length) {
      await this.fetchSubjectsByTopics(this.form.topicCodes);
    }
  } catch (e) {
    console.error("Failed to load topics for subject", e);
    this.form.topicCodes = [];
  } finally {
    this.loadingTopics = false;
  }
},
    removeSession(index) {
      if ((this.form.sessions || []).length > 1) {
        this.form.sessions.splice(index, 1);
      }
    },

    async fetchLookupData() {
      try {
        const res = await apiClient.get("/CreateCourse/lookup");

        this.lookupData = {
          trainingCenters:
            res.data.trainingCenters?.$values ||
            res.data.TrainingCenters?.$values ||
            res.data.TrainingCenters ||
            [],
          regions:
            res.data.regions?.$values ||
            res.data.Regions?.$values ||
            res.data.Regions ||
            [],
          topics:
            res.data.topics?.$values ||
            res.data.Topics?.$values ||
            res.data.Topics ||
            [],
          instructors:
            res.data.instructors?.$values ||
            res.data.Instructors?.$values ||
            res.data.Instructors ||
            [],
          deliverables:
            res.data.deliverables?.$values ||
            res.data.Deliverables?.$values ||
            res.data.Deliverables ||
            [],
          formats:
            res.data.formats?.$values ||
            res.data.Formats?.$values ||
            res.data.Formats ||
            []
        };
      } catch (err) {
        console.error("❌ Failed to fetch lookup data", err);
      }
    },

    async fetchSubjectsByTopics(topicCodes) {
      try {
        const payload = {
          topicCodes: (topicCodes || []).map(Number).filter((n) => !Number.isNaN(n))
        };

        const res = await apiClient.post("/CreateCourse/subjectsByTopics", payload);
        this.filteredSubjects = res.data?.$values || res.data || [];

        const stillValid = (this.filteredSubjects || []).some(
          (x) => String(x.subjectSysId) === String(this.form.courseTitle)
        );
        if (!stillValid) this.form.courseTitle = "";
      } catch (err) {
        console.error("Failed to load subjects by topics", err);
        this.filteredSubjects = [];
        this.form.courseTitle = "";
      }
    },

    async populateForm(c) {
      // basic fields
      this.form.trainingCenter = String(c.siteSysId ?? "");
      this.form.region = c.region != null ? String(c.region) : "";
      this.form.courseTitle = c.subjectSysId != null ? String(c.subjectSysId) : "";

      this.form.instructor1 = c.instructor1 != null ? String(c.instructor1) : "";
      this.form.instructor2 = c.instructor2 != null ? String(c.instructor2) : "";

      this.form.startDate = c.courseDate?.split("T")[0] || "";
      this.form.endDate = c.endDate?.split("T")[0] || "";
      this.form.startTime = c.courseTimeBegin?.substring(11, 16) || "";
      this.form.endTime = c.courseTimeEnd?.substring(11, 16) || "";

      this.form.regDeadline = c.regDeadLine?.split("T")[0] || "";
      this.form.maxSeats = c.maxSeats ?? "";

      this.form.trainingLocation = c.trainingLocation || "";
      this.form.virtualUrl = c.virtualUrl || "";
      this.form.deliverables = c.deliverable != null ? String(c.deliverable) : "";
      this.form.format = c.format != null ? String(c.format) : "";

      this.form.fundingType = c.rtc ? "RTC" : c.coe ? "COE" : "Others";
      this.form.hideCourse = !!c.hidden;
      this.form.courseSchedule = c.information || "";

      this.form.isMultiSession = !!c.isMultiSession;
      this.form.baseHours = c.baseHours || c.BaseHours || 0;

      this.form.markAsNewUntil =
        c.markAsNewUntil?.split("T")[0] ||
        c.MarkAsNewUntil?.split("T")[0] ||
        "";

      // sessions
      const rawSessions = c.sessions?.$values || c.sessions || [];
      this.form.sessions = rawSessions.length
        ? rawSessions.map((s) => ({
            date: s.sessionDate?.split("T")[0] || "",
            startTime: s.startTime?.substring(0, 5) || "",
            endTime: s.endTime?.substring(0, 5) || "",
            url: s.sessionUrl || "",
            trainingLocation: s.trainingLocation ?? ""
          }))
        : [{ date: "", startTime: "", endTime: "", url: "", trainingLocation: "" }];

      // ✅ load topics for this subject from backend (recommended)
      if (c.subjectSysId) {
        try {
          const tRes = await apiClient.get(`/CreateCourse/topicsBySubject/${c.subjectSysId}`);
          const codes = tRes.data?.$values || tRes.data || [];
          this.form.topicCodes = (codes || []).map((x) => String(x));

          if ((this.form.topicCodes || []).length) {
            await this.fetchSubjectsByTopics(this.form.topicCodes);
          }
        } catch (e) {
          console.error("Failed to load topics for subject", e);
          this.form.topicCodes = [];
        }
      }
    },

    async submitUpdate() {
      const topicCodes = (this.form.topicCodes || [])
        .map(Number)
        .filter((n) => !Number.isNaN(n));

      if (topicCodes.length === 0) {
        this.topicError = true;
        alert("Please select at least one topic.");
        return;
      }
      this.topicError = false;

      try {
        const n = (v) => (v === "" || v === null || v === undefined ? null : Number(v));

        const coursePayload = {
          courseSysId: this.originalCourse.courseSysId,
          siteSysId: n(this.form.trainingCenter),
          region: n(this.form.region),
          subjectSysId: n(this.form.courseTitle),
          instructor1: n(this.form.instructor1),
          instructor2: n(this.form.instructor2),

          courseDate: this.form.startDate ? new Date(this.form.startDate).toISOString() : null,
          endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null,

          courseTimeBegin: this.form.startTime
            ? new Date(`${this.form.startDate}T${this.form.startTime}:00`).toISOString()
            : null,
          courseTimeEnd: this.form.endTime
            ? new Date(`${this.form.endDate}T${this.form.endTime}:00`).toISOString()
            : null,

          regDeadLine: this.form.regDeadline ? new Date(this.form.regDeadline).toISOString() : null,

          markAsNewUntil: this.form.markAsNewUntil
            ? new Date(this.form.markAsNewUntil).toISOString()
            : null,

          maxSeats: n(this.form.maxSeats),
          trainingLocation: this.form.trainingLocation || null,
          virtualUrl: this.form.virtualUrl || null,
          deliverable: n(this.form.deliverables),
          format: n(this.form.format),

          rtc: this.form.fundingType === "RTC",
          coe: this.form.fundingType === "COE",
          otherFund: this.form.fundingType === "Others",
          hidden: !!this.form.hideCourse,

          information: this.form.courseSchedule || null,
          isMultiSession: !!this.form.isMultiSession,
          baseHours: this.calculatedBaseHours,
          dateModified: new Date().toISOString()
        };

        const sessions = this.form.isMultiSession
          ? (this.form.sessions || [])
              .filter((s) => s.date && s.startTime && s.endTime)
              .map((s) => ({
                sessionDate: new Date(s.date).toISOString(),
                startTime: s.startTime,
                endTime: s.endTime,
                sessionUrl: s.url ?? "",
                trainingLocation: s.trainingLocation ?? ""
              }))
          : [];

const requestPayload = { 
  course: coursePayload, 
  sessions,
  topicCodes // ✅ send to backend
};
        const courseSysId = this.originalCourse.courseSysId;
        await apiClient.put(`/CourseAdmin/update/${courseSysId}`, requestPayload, {
          headers: { "Content-Type": "application/json" }
        });

        alert("Course updated successfully!");
        this.$emit("updated");
        this.$emit("close");
      } catch (err) {
        console.error("Error updating course:", err?.response?.data || err);
        alert("Failed to update course.");
      }
    }
  }
};</script>

<style scoped>
    /* Overlay */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: radial-gradient(circle at top, rgba(15, 23, 42, 0.3), rgba(15, 23, 42, 0.75));
        backdrop-filter: blur(6px);
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 24px;
        z-index: 999;
    }

    /* Modal Shell */
    .modal {
        position: relative;
        background-color: #fff;
        border-radius: 24px;
        width: 1100px;
        max-width: 100%;
        max-height: 90vh;
        overflow-y: auto;
        padding: 32px;
        box-shadow: 0 24px 60px rgba(15, 23, 42, 0.25), 0 0 0 1px rgba(148, 163, 184, 0.35);
        font-family: system-ui, -apple-system, 'Segoe UI', sans-serif;
    }

    /* Close Button */
    .close-btn {
        position: absolute;
        top: 14px;
        right: 16px;
        background: rgba(255, 255, 255, 0.9);
        border: none;
        border-radius: 50%;
        font-size: 20px;
        width: 34px;
        height: 34px;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.15);
        transition: all 0.2s ease;
    }

        .close-btn:hover {
            background: #ffe6e6;
            color: #c62828;
            transform: scale(1.05);
        }

    /* PURPLE HEADER (same style / color as Schedule Course modal) */
    .modal-header-purple {
        background: #43285d; /* single brand color */
        padding: 26px 32px;
        margin: -32px -32px 24px -32px; /* stretch to modal edges */
        border-top-left-radius: 24px;
        border-top-right-radius: 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 20px;
        box-shadow: inset 0 -1px 0 rgba(255, 255, 255, 0.08);
    }

        .modal-header-purple h2 {
            color: #ffffff;
            font-size: 22px;
            font-weight: 700;
            margin: 0;
            letter-spacing: -0.3px;
        }

        .modal-header-purple p {
            color: #ddd7f1;
            margin: 4px 0 0 0;
            font-size: 13px;
        }

    /* Badge on purple header */
    .modal-badge {
        background: rgba(255, 255, 255, 0.12);
        color: #ffffff;
        padding: 8px 16px;
        border-radius: 999px;
        font-size: 11px;
        font-weight: 600;
        white-space: nowrap;
        border: 1px solid rgba(255, 255, 255, 0.18);
    }

    /* Section Cards */
    .section-card {
        background: #ffffff;
        border-radius: 18px;
        padding: 20px;
        border: 1px solid #e5e7eb;
        margin-bottom: 20px;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06), 0 0 0 1px rgba(148, 163, 184, 0.08);
    }

    .section-header {
        margin-bottom: 14px;
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
    }

    .section-header-inline {
        align-items: center;
    }

    .section-header h3 {
        margin: 0;
        font-size: 16px;
        font-weight: 600;
        color: #0f172a;
    }

    .section-header p {
        margin: 2px 0 0;
        font-size: 12px;
        color: #6b7280;
    }

    /* Form Layout */
    .form-container {
        display: flex;
        gap: 20px;
        flex-wrap: wrap;
    }

    .form-column {
        flex: 1 1 48%;
    }

    .form-column-full {
        flex: 1 1 100%;
    }

    /* Inputs */
    .form-group {
        margin-bottom: 16px;
    }

    label {
        font-size: 13px;
        font-weight: 600;
        margin-bottom: 4px;
        display: block;
        color: #111827;
    }

    input,
    select,
    textarea {
        width: 100%;
        padding: 10px;
        border-radius: 10px;
        border: 1px solid #d1d5db;
        background: #f9fafb;
        font-size: 13px;
        transition: all 0.18s ease;
    }

        input:focus,
        select:focus,
        textarea:focus {
            border-color: #43285d;
            background: #ffffff;
            outline: none;
            box-shadow: 0 0 0 1px rgba(67, 40, 93, 0.5), 0 0 0 4px rgba(67, 40, 93, 0.15);
        }

    textarea {
        resize: vertical;
    }

    /* Inline time fields */
    .inline-group {
        display: flex;
        gap: 12px;
    }

    .inline-field {
        flex: 1;
    }

    /* Session group */
    .session-group {
        background: #fafafa;
        border: 1px dashed #d4d4d8;
        padding: 14px;
        border-radius: 14px;
        margin-bottom: 14px;
        display: flex;
        flex-wrap: wrap;
        gap: 14px;
    }

        .session-group .form-group {
            flex: 1 1 46%;
        }

    .session-actions {
        flex: 1 1 100%;
        display: flex;
        justify-content: flex-end;
    }

    /* Checkbox / Toggle */
    input[type='checkbox'] {
        accent-color: #43285d;
        cursor: pointer;
    }

    .checkbox-row {
        display: flex;
        align-items: center;
        gap: 8px;
    }

        .checkbox-row label {
            margin: 0;
        }

    .toggle-label {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        color: #374151;
    }

        .toggle-label input[type='checkbox'] {
            width: 18px;
            height: 18px;
        }

    /* Buttons */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 12px;
        margin-top: 20px;
        padding-top: 12px;
        border-top: 1px solid #e5e7eb;
    }

    /* PRIMARY BUTTON – same purple #43285D as header */
    .btn-primary {
        background: #43285d; /* brand color */
        padding: 10px 24px;
        color: #ffffff;
        border: none;
        border-radius: 999px;
        cursor: pointer;
        font-size: 14px;
        font-weight: 600;
        transition: all 0.18s ease;
        box-shadow: 0 6px 14px rgba(67, 40, 93, 0.4);
    }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 22px rgba(67, 40, 93, 0.55);
        }

    /* Secondary / danger */
    .btn-secondary {
        background: #e5e7eb;
        padding: 10px 22px;
        border-radius: 999px;
        cursor: pointer;
        border: none;
        font-size: 13px;
        font-weight: 500;
        color: #111827;
        transition: background-color 0.12s ease, transform 0.12s ease;
    }

        .btn-secondary:hover {
            background-color: #d1d5db;
            transform: translateY(-0.5px);
        }

    .btn-danger {
        background: #fee2e2;
        color: #b91c1c;
    }

    /* Smaller screens */
    @media (max-width: 900px) {
        .modal {
            width: 100%;
            padding: 20px 16px 16px;
            border-radius: 18px;
        }

        .form-container {
            flex-direction: column;
        }

        .button-group {
            flex-direction: column-reverse;
            align-items: stretch;
        }

        .btn-primary,
        .btn-secondary {
            width: 100%;
        }
    }
    .topic-multi {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 10px;
        margin-top: 8px;
    }

    .topic-item {
        display: flex;
        align-items: center;
        gap: 10px;
        padding: 10px 12px;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background: #f9fafb;
        cursor: pointer;
    }

        .topic-item input {
            width: 16px;
            height: 16px;
        }

    .required-border {
        border: 1px solid #ef4444 !important;
        border-radius: 10px;
        padding: 10px;
    }

    .error-text {
        display: block;
        margin-top: 8px;
        color: #ef4444;
        font-size: 12px;
    }

    .hint {
        display: block;
        margin-top: 8px;
        opacity: 0.8;
        font-size: 12px;
    }
</style>