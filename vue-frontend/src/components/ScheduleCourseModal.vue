<template>
    <div v-if="isOpen" class="modal-overlay">
        <div class="modal">
            <h2>Schedule a New Course</h2>

            <form @submit.prevent="submitCourse">
                <div class="form-container">
                    <!-- Left Column -->
                    <div class="form-column">
                        <div class="form-group">
                            <label>Training Center *</label>
                            <select v-model="form.trainingCenter" required>
                                <option value="">-- Select --</option>
                                <option v-for="center in lookupData.trainingCenters"
                                        :key="center.siteSysId"
                                        :value="center.siteSysId">
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
                                        :value="region.code">
                                    {{ region.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Category *</label>
                            <select v-model="form.category" required>
                                <option value="">-- Select --</option>
                                <option v-for="category in lookupData.categories"
                                        :key="category.code"
                                        :value="category.code">
                                    {{ category.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Course Title *</label>
                            <select v-model="form.courseTitle" required>
                                <option value="">-- Select --</option>
                                <option v-for="subject in filteredSubjects"
                                        :key="subject.subjectSysId"
                                        :value="subject.subjectSysId">
                                    {{ subject.courseTitle }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>1st Instructor</label>
                            <select v-model="form.instructor1">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors"
                                        :key="instructor.instructorSysId"
                                        :value="instructor.instructorSysId">
                                    {{ instructor.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>2nd Instructor</label>
                            <select v-model="form.instructor2">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors"
                                        :key="instructor.instructorSysId"
                                        :value="instructor.instructorSysId">
                                    {{ instructor.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Registration Deadline *</label>
                            <input type="date" v-model="form.regDeadline" required />
                        </div>
                    </div>

                    <!-- Right Column -->
                    <div class="form-column">
                        <div class="form-group">
                            <label>Course Start Date *</label>
                            <input type="date" v-model="form.startDate" required />
                        </div>

                        <div class="form-group">
                            <label>Course End Date *</label>
                            <input type="date" v-model="form.endDate" required />
                        </div>

                        <div class="form-group">
                            <label>Begin Time *</label>
                            <input type="time" v-model="form.startTime" required />
                        </div>

                        <div class="form-group">
                            <label>End Time *</label>
                            <input type="time" v-model="form.endTime" required />
                        </div>

                        <div class="form-group">
                            <label>Maximum Seats *</label>
                            <input type="number" v-model="form.maxSeats" min="1" required />
                        </div>

                        <div class="form-group">
                            <label>Training Center Location</label>
                            <input type="text" v-model="form.trainingLocation" />
                        </div>

                        <div class="form-group">
                            <label>Format *</label>
                            <select v-model="form.format" required>
                                <option value="">-- Select --</option>
                                <option v-for="format in lookupData.formats"
                                        :key="format.code"
                                        :value="format.code">
                                    {{ format.value }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>

                <!-- Multi-session toggle -->
                <div class="form-column-full">
                    <div class="form-group">
                        <label>
                            <input type="checkbox" v-model="form.isMultiSession" />
                            Multi-day / Multi-session Course
                        </label>
                    </div>
                </div>

                <!-- Multi-session entries -->
                <div class="form-column-full" v-if="form.isMultiSession">
                    <h4>Course Sessions</h4>

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
                            <input type="url" v-model="session.url" placeholder="Session URL" />
                        </div>

                        <!-- ❌ Remove Session Button -->
                        <div class="form-group" style="align-self: flex-end;">
                            <button v-if="form.sessions.length > 1"
                                    type="button"
                                    class="btn-secondary"
                                    @click="removeSession(index)">
                                ❌ Remove
                            </button>
                        </div>

                        <!-- ➕ Add Session Button -->
                        <div class="form-group" v-if="index === form.sessions.length - 1 && form.sessions.length < 4">
                            <button type="button" class="btn-secondary" @click="addSession">
                                ➕ Add Session
                            </button>
                        </div>
                    </div>

                    <!-- Automatically show 1st session block when checkbox is checked -->
                    <div v-if="form.sessions.length === 0">
                        <button type="button" class="btn-secondary" @click="addSession">
                            ➕ Add First Session
                        </button>
                    </div>
                </div>

                <!-- Remaining fields -->
                <div class="form-column-full">
                    <div class="form-group">
                        <label># of Deliverables *</label>
                        <select v-model="form.deliverables" required>
                            <option value="">-- Select --</option>
                            <option v-for="deliverable in lookupData.deliverables"
                                    :key="deliverable.id"
                                    :value="deliverable.id">
                                {{ deliverable.value }}
                            </option>
                        </select>
                    </div>

                    <div class="form-group">
                        <label>Funding Type</label>
                        <div>
                            <input type="radio" id="rtc" value="RTC" v-model="form.fundingType" />
                            <label for="rtc">RTC</label>

                            <input type="radio" id="coe" value="COE" v-model="form.fundingType" />
                            <label for="coe">COE</label>

                            <input type="radio"
                                   id="others"
                                   value="Others"
                                   v-model="form.fundingType" />
                            <label for="others">Others</label>
                        </div>
                    </div>

                    <div class="form-group">
                        <input type="checkbox" v-model="form.hideCourse" />
                        <label>Hide Course</label>
                    </div>

                    <div class="form-group">
                        <label>Course Schedule</label>
                        <textarea v-model="form.courseSchedule"></textarea>
                    </div>
                </div>

                <!-- Buttons -->
                <div class="button-group">
                    <button type="submit" class="btn-primary">Submit</button>
                    <button type="button" class="btn-secondary" @click="closeModal">
                        Cancel
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>


<script>import apiClient from "@/axios.js";

export default {
  name: "ScheduleCourseModal",
  props: { isOpen: Boolean },
  emits: ["close", "submit"],
  data() {
  return {
    form: {
      trainingCenter: "", region: "", category: "", courseTitle: "", instructor1: "",
      instructor2: "", startDate: "", endDate: "", startTime: "", endTime: "",
      regDeadline: "", maxSeats: "", trainingLocation: "", deliverables: "",
      format: "", fundingType: "", hideCourse: false, courseSchedule: "",
      isMultiSession: false,
      sessions: [
        
      ]
    }, // <-- make sure this closing brace is correct
    lookupData: {
      trainingCenters: [], regions: [], categories: [], instructors: [],
      deliverables: [], formats: []
    },
    filteredSubjects: []
  };
},
  watch: {
    isOpen(newVal) {
    if (newVal) {
      this.resetForm();
      this.fetchLookupData();
    }
    },
    'form.category'(newCategory) {
      if (newCategory) {
        this.fetchSubjectsByCategory(newCategory);
      } else {
        this.filteredSubjects = [];
        this.form.courseTitle = "";
      }
    }
  },
  methods: {
      removeSession(index) {
  this.form.sessions.splice(index, 1);
},
      resetForm() {
    this.form = {
      trainingCenter: "", region: "", category: "", courseTitle: "", instructor1: "",
      instructor2: "", startDate: "", endDate: "", startTime: "", endTime: "",
      regDeadline: "", maxSeats: "", trainingLocation: "", deliverables: "",
      format: "", fundingType: "", hideCourse: false, courseSchedule: "",
      isMultiSession: false,
      sessions: []
    };
    this.filteredSubjects = [];
  },
      addSession() {
  if (this.form.sessions.length < 4) {
    this.form.sessions.push({ date: "", startTime: "", endTime: "", url: "" });
  }
},
    async fetchLookupData() {
      try {
        const response = await apiClient.get("/CreateCourse/lookup");

        this.lookupData = {
          trainingCenters: response.data.trainingCenters?.$values || [],
          regions: response.data.regions?.$values || [],
          categories: response.data.categories?.$values || [],
          instructors: response.data.instructors?.$values || [],
          deliverables: response.data.deliverables?.$values || [],
          formats: response.data.formats?.$values || []
        };
      } catch (error) {
        console.error("Error fetching lookup data:", error);
      }
    },
    async fetchSubjectsByCategory(categoryCode) {
      try {
        const res = await apiClient.get(`/CreateCourse/subjectsByCategory/${categoryCode}`);
        this.filteredSubjects = res.data?.$values || [];
      } catch (err) {
        console.error("Failed to load subjects by category", err);
      }
    },
    closeModal() {
      this.$emit("close");
    },
    async submitCourse() {
  try {
    const courseTimeBegin = this.form.startDate && this.form.startTime
      ? new Date(`${this.form.startDate}T${this.form.startTime}:00Z`).toISOString()
      : null;

    const courseTimeEnd = this.form.endDate && this.form.endTime
      ? new Date(`${this.form.endDate}T${this.form.endTime}:00Z`).toISOString()
      : null;

    const course = {
      siteSysId: this.form.trainingCenter,
      subjectSysId: this.form.courseTitle,
      courseDate: this.form.startDate ? new Date(this.form.startDate).toISOString() : null,
      endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null,
      courseTimeBegin,
      courseTimeEnd,
      regDeadLine: this.form.regDeadline ? new Date(this.form.regDeadline).toISOString() : null,
      instructor1: this.form.instructor1 || null,
      instructor2: this.form.instructor2 || null,
      trainingLocation: this.form.trainingLocation,
      deliverable: this.form.deliverables,
      maxSeats: this.form.maxSeats,
      format: this.form.format,
      region: this.form.region,
      information: this.form.courseSchedule,
      rtc: this.form.fundingType === "RTC",
      coe: this.form.fundingType === "COE",
      otherFund: this.form.fundingType === "Others",
      hidden: this.form.hideCourse,
      isMultiSession: this.form.isMultiSession,
      delivered: false,
      cancelled: false,
      approve: null,
      approveDt: null,
      disapprove: null,
      disapproveDt: null,
      disApprvNotes: null,
      dateEntered: new Date().toISOString(),
      dateModified: new Date().toISOString()
    };

    // 👇 Wrap both course and sessions together
    const requestPayload = {
      course,
      sessions: this.form.isMultiSession
        ? this.form.sessions.map(s => ({
            sessionDate: s.date,
            startTime: s.startTime,
            endTime: s.endTime,
            sessionUrl: s.url
          }))
        : []
    };

    await apiClient.post("/CreateCourse/schedule", requestPayload);
    alert("Course scheduled successfully!");
    this.closeModal();
  } catch (error) {
    console.error("Error scheduling course:", error);
    alert("Failed to schedule course. Please try again.");
  }
}
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
        z-index: 999;
    }

    .modal {
        background: #ffffff;
        padding: 36px;
        border-radius: 18px;
        width: 960px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
        font-family: 'Segoe UI', sans-serif;
        animation: fadeIn 0.3s ease;
    }

        .modal h2 {
            font-size: 28px;
            font-weight: 600;
            margin-bottom: 30px;
            text-align: center;
            color: #1f1f1f;
        }

    .form-container {
        display: flex;
        flex-wrap: wrap;
        gap: 24px;
    }

    .form-column {
        flex: 1 1 45%;
    }

    .form-column-full {
        flex: 1 1 100%;
        margin-top: 20px;
    }

    .form-group {
        margin-bottom: 18px;
    }

    label {
        font-weight: 600;
        margin-bottom: 8px;
        display: block;
        color: #444;
        font-size: 15px;
    }

    input,
    select,
    textarea {
        width: 100%;
        padding: 12px 14px;
        font-size: 15px;
        border: 1px solid #d0d0d0;
        border-radius: 10px;
        background-color: #fafafa;
        transition: all 0.2s ease;
        box-sizing: border-box;
    }

        input:focus,
        select:focus,
        textarea:focus {
            border-color: #3f51b5;
            background-color: #fff;
            outline: none;
            box-shadow: 0 0 0 2px rgba(63, 81, 181, 0.15);
        }

    textarea {
        resize: vertical;
        min-height: 80px;
    }

    input[type="radio"],
    input[type="checkbox"] {
        margin-right: 8px;
    }

    .form-group input[type="radio"] + label,
    .form-group input[type="checkbox"] + label {
        display: inline-block;
        margin-right: 20px;
        font-weight: 500;
        font-size: 14px;
        color: #333;
    }

    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 16px;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 12px 24px;
        border: none;
        font-size: 15px;
        border-radius: 8px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-primary:hover {
            background-color: #2f3e94;
        }

    .btn-secondary {
        background-color: #f0f0f0;
        color: #333;
        padding: 12px 24px;
        border: 1px solid #ccc;
        font-size: 15px;
        border-radius: 8px;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-secondary:hover {
            background-color: #e2e2e2;
        }

    /* Subtle fade animation */
    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    /* Responsive fallback */
    @media (max-width: 768px) {
        .modal {
            width: 95%;
            padding: 24px;
        }

        .form-container {
            flex-direction: column;
        }

        .button-group {
            flex-direction: column;
            align-items: stretch;
        }

        .btn-primary,
        .btn-secondary {
            width: 100%;
        }
    }
    .session-group {
        border: 1px solid #ccc;
        padding: 16px;
        margin-bottom: 16px;
        border-radius: 12px;
        background-color: #f9f9f9;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
        display: flex;
        flex-wrap: wrap;
        gap: 16px;
    }

        .session-group .form-group {
            flex: 1 1 45%;
        }

    input[type="checkbox"],
    input[type="radio"] {
        accent-color: #3f51b5;
        width: 18px;
        height: 18px;
        cursor: pointer;
    }

        input[type="checkbox"] + label,
        input[type="radio"] + label {
            font-size: 15px;
            cursor: pointer;
        }

    .form-group > label > input[type="checkbox"] {
        margin-right: 10px;
    }

    /* Make radio buttons inline */
    .form-group input[type="radio"] {
        margin-left: 10px;
        margin-right: 5px;
    }
</style>
