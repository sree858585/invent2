<template>
    <div v-if="isOpen" class="modal-overlay">
        <div class="modal">
            <!-- Close Button -->
            <button class="modal-close" @click="closeModal">×</button>
            <!-- Header -->
            <!-- HEADER (Final polished version) -->
            <div class="modal-header-purple">
                <div class="header-left">
                    <h2>Schedule a New Course</h2>
                    <p>Configure course details, timing, format, and multi-session options in one place.</p>
                </div>

                <span class="modal-badge">Admin • Course Scheduler</span>
            </div>

            <form @submit.prevent="submitCourse">
                <!-- SECTION: Course Details -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Course Details</h3>
                            <p>Choose where this training will be hosted and what content it covers.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <!-- Left Column -->
                        <div class="form-column">
                            <div class="form-group">
                                <label>Training Center <span class="required">*</span></label>
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
                                <label>Region <span class="required">*</span></label>
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
                                <label>Category <span class="required">*</span></label>
                                <select v-model="form.category" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="category in lookupData.categories"
                                            :key="category.code"
                                            :value="category.code">
                                        {{ category.value }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <!-- Right Column -->
                        <div class="form-column">
                            <div class="form-group">
                                <label>Course Title <span class="required">*</span></label>
                                <select v-model="form.courseTitle" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="subject in filteredSubjects"
                                            :key="subject.subjectSysId"
                                            :value="subject.subjectSysId">
                                        {{ subject.courseTitle }}
                                    </option>
                                </select>
                            </div>

                            <!-- Kept commented as requested -->
                            <!--<div class="form-group">
                      <label>1st Instructor</label>
                      <select v-model="form.instructor1">
                        <option value="">-- Select --</option>
                        <option
                          v-for="instructor in lookupData.instructors"
                          :key="instructor.instructorSysId"
                          :value="instructor.instructorSysId"
                        >
                          {{ instructor.name }}
                        </option>
                      </select>
                    </div>

                    <div class="form-group">
                      <label>2nd Instructor</label>
                      <select v-model="form.instructor2">
                        <option value="">-- Select --</option>
                        <option
                          v-for="instructor in lookupData.instructors"
                          :key="instructor.instructorSysId"
                          :value="instructor.instructorSysId"
                        >
                          {{ instructor.name }}
                        </option>
                      </select>
                    </div>-->

                            <div class="form-group">
                                <label>Registration Deadline <span class="required">*</span></label>
                                <input type="date" v-model="form.regDeadline" required />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Schedule & Capacity -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Schedule & Capacity</h3>
                            <p>Set dates, times, and overall seat capacity for this course.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <div class="form-column">
                            <div class="form-group">
                                <label>Course Start Date <span class="required">*</span></label>
                                <input type="date" v-model="form.startDate" required />
                            </div>

                            <div class="form-group">
                                <label>Course End Date <span class="required">*</span></label>
                                <input type="date" v-model="form.endDate" required />
                            </div>
                        </div>

                        <div class="form-column">
                            <div class="form-group inline-group">
                                <div class="inline-field">
                                    <label>Begin Time <span class="required">*</span></label>
                                    <input type="time" v-model="form.startTime" required />
                                </div>

                                <div class="inline-field">
                                    <label>End Time <span class="required">*</span></label>
                                    <input type="time" v-model="form.endTime" required />
                                </div>
                            </div>

                            <div class="form-group">
                                <label>Training Capacity <span class="required">*</span></label>
                                <input type="number" v-model="form.maxSeats" min="1" required />
                            </div>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Format & Delivery -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Format & Delivery</h3>
                            <p>Choose how this course is delivered and capture the correct location or URL.</p>
                        </div>
                    </div>

                    <div class="form-container">
                        <div class="form-column">
                            <div class="form-group">
                                <label>Format <span class="required">*</span></label>
                                <select v-model="form.format" required>
                                    <option value="">-- Select --</option>
                                    <!-- Only allowed formats -->
                                    <option v-for="format in filteredFormats"
                                            :key="format.code"
                                            :value="format.code">
                                        {{ format.value }}
                                    </option>
                                </select>
                            </div>

                            <!-- FACE TO FACE ONLY -->
                            <div class="form-group" v-if="form.format === 1">
                                <label>Training Center Location <span class="required">*</span></label>
                                <input type="text"
                                       v-model="form.trainingLocation"
                                       placeholder="Enter room / building / address"
                                       required />
                            </div>
                        </div>

                        <div class="form-column">
                            <!-- VIRTUAL FORMATS -->
                            <div class="form-group" v-if="[3, 4, 5, 6].includes(form.format)">
                                <label>Webinar / Webcast URL <span class="required">*</span></label>
                                <input type="text"
                                       v-model="form.virtualUrl"
                                       placeholder="Enter webinar or webcast link"
                                       required />
                            </div>

                            <!--<div class="form-group">
                      <label># of Deliverables *</label>
                      <select v-model="form.deliverables" required>
                        <option value="">-- Select --</option>
                        <option
                          v-for="deliverable in lookupData.deliverables"
                          :key="deliverable.id"
                          :value="deliverable.id"
                        >
                          {{ deliverable.value }}
                        </option>
                      </select>
                    </div>-->
                            <!--<div class="form-group">
                      <label>Funding Type</label>
                      <div class="radio-row">
                        <div class="radio-item">
                          <input type="radio" id="rtc" value="RTC" v-model="form.fundingType" />
                          <label for="rtc">RTC</label>
                        </div>

                        <div class="radio-item">
                          <input type="radio" id="coe" value="COE" v-model="form.fundingType" />
                          <label for="coe">COE</label>
                        </div>

                        <div class="radio-item">
                          <input type="radio" id="others" value="Others" v-model="form.fundingType" />
                          <label for="others">Others</label>
                        </div>
                      </div>
                    </div>-->
                        </div>
                    </div>
                </section>

                <!-- SECTION: Multi-session -->
                <section class="section-card">
                    <div class="section-header section-header-inline">
                        <div>
                            <h3>Multi-day / Multi-session</h3>
                            <p>Define separate sessions if this course spans multiple days or time blocks.</p>
                        </div>

                        <label class="toggle-label">
                            <input type="checkbox" v-model="form.isMultiSession" />
                            <span>Enable multi-session schedule</span>
                        </label>
                    </div>

                    <!-- Multi-session entries -->
                    <div class="form-column-full" v-if="form.isMultiSession">
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

                            <div class="form-group">
                                <label>Training Location</label>
                                <input type="text"
                                       v-model="session.trainingLocation"
                                       placeholder="Enter training center / location" />
                            </div>

                            <!-- ❌ Remove Session Button -->
                            <div class="form-group session-actions" v-if="form.sessions.length > 1">
                                <button type="button"
                                        class="btn-secondary btn-danger"
                                        @click="removeSession(index)">
                                    ❌ Remove Session
                                </button>
                            </div>

                            <!-- ➕ Add Session Button -->
                            <div class="form-group session-actions"
                                 v-if="index === form.sessions.length - 1 && form.sessions.length < 4">
                                <button type="button" class="btn-secondary" @click="addSession">
                                    ➕ Add Another Session
                                </button>
                            </div>
                        </div>

                        <!-- Automatically show 1st session block when checkbox is checked -->
                        <div v-if="form.sessions.length === 0" class="session-empty">
                            <button type="button" class="btn-secondary" @click="addSession">
                                ➕ Add First Session
                            </button>
                        </div>
                    </div>
                </section>

                <!-- SECTION: Visibility & Notes -->
                <section class="section-card">
                    <div class="section-header">
                        <div>
                            <h3>Visibility & Notes</h3>
                            <p>Control catalog visibility and add any internal notes or schedule details.</p>
                        </div>
                    </div>

                    <div class="form-column-full">
                        <div class="form-group checkbox-row">
                            <input type="checkbox" id="hideCourse" v-model="form.hideCourse" />
                            <label for="hideCourse">Hide course from public listing</label>
                        </div>

                        <div class="form-group">
                            <label>Additional Notes</label>
                            <textarea v-model="form.courseSchedule"
                                      placeholder="Add agenda, special instructions, or internal notes..."></textarea>
                        </div>
                    </div>
                </section>

                <!-- Footer Buttons -->
                <div class="button-group">
                    <button type="button" class="btn-secondary" @click="closeModal">
                        Cancel
                    </button>
                    <button type="submit" class="btn-primary">
                        Schedule Course
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
                    trainingCenter: "",
                    region: "",
                    category: "",
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
                    deliverables: "",
                    format: null,
                    fundingType: "",
                    hideCourse: false,
                    courseSchedule: "",
                    virtualUrl: "",
                    isMultiSession: false,
                    sessions: []
                },
                lookupData: {
                    trainingCenters: [],
                    regions: [],
                    categories: [],
                    instructors: [],
                    deliverables: [],
                    formats: []
                },
                filteredSubjects: []
            };
        },
        computed: {
            filteredFormats() {
                return this.lookupData.formats.filter((f) => f.code !== 2);
            }
        },
        watch: {
            isOpen(newVal) {
                if (newVal) {
                    this.resetForm();
                    this.fetchLookupData();
                }
            },
            "form.category"(newCategory) {
                if (newCategory) {
                    this.fetchSubjectsByCategory(newCategory);
                } else {
                    this.filteredSubjects = [];
                    this.form.courseTitle = "";
                }
            },
            // Optional: auto-add first session when toggle turned ON
            "form.isMultiSession"(val) {
                if (val && this.form.sessions.length === 0) {
                    this.addSession();
                }
                if (!val) {
                    this.form.sessions = [];
                }
            }
        },
        methods: {
            removeSession(index) {
                this.form.sessions.splice(index, 1);
            },
            resetForm() {
                this.form = {
                    trainingCenter: "",
                    region: "",
                    category: "",
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
                    deliverables: "",
                    format: null,
                    fundingType: "",
                    hideCourse: false,
                    courseSchedule: "",
                    virtualUrl: "",
                    isMultiSession: false,
                    sessions: []
                };
                this.filteredSubjects = [];
            },
            addSession() {
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
                    const courseTimeBegin =
                        this.form.startDate && this.form.startTime
                            ? new Date(`${this.form.startDate}T${this.form.startTime}:00Z`).toISOString()
                            : null;

                    const courseTimeEnd =
                        this.form.endDate && this.form.endTime
                            ? new Date(`${this.form.endDate}T${this.form.endTime}:00Z`).toISOString()
                            : null;

                    const course = {
                        SiteSysId: this.form.trainingCenter,
                        SubjectSysId: this.form.courseTitle,
                        CourseDate: this.form.startDate
                            ? new Date(this.form.startDate).toISOString()
                            : null,
                        EndDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null,
                        CourseTimeBegin: courseTimeBegin,
                        CourseTimeEnd: courseTimeEnd,
                        RegDeadLine: this.form.regDeadline
                            ? new Date(this.form.regDeadline).toISOString()
                            : null,
                        Instructor1: this.form.instructor1 || null,
                        Instructor2: this.form.instructor2 || null,
                        TrainingLocation: this.form.trainingLocation,
                        VirtualUrl: this.form.virtualUrl,
                        deliverable: this.form.deliverables ? Number(this.form.deliverables) : null,
                        MaxSeats: this.form.maxSeats,
                        Format: Number(this.form.format),
                        Region: Number(this.form.region),
                        Information: this.form.courseSchedule,
                        Rtc: this.form.fundingType === "RTC",
                        Coe: this.form.fundingType === "COE",
                        OtherFund: this.form.fundingType === "Others",
                        Hidden: this.form.hideCourse,
                        IsMultiSession: this.form.isMultiSession,
                        Delivered: false,
                        Cancelled: false
                    };

                    const requestPayload = {
                        course,
                        sessions: this.form.isMultiSession
                            ? this.form.sessions.map((s) => ({
                                SessionDate: s.date,
                                StartTime: s.startTime,
                                EndTime: s.endTime,
                                SessionUrl: s.url,
                                TrainingLocation: s.trainingLocation
                            }))
                            : []
                    };

                    if (this.form.format === 1 && !this.form.trainingLocation.trim()) {
                        alert("Training Center Location is required for Face-to-face format.");
                        return;
                    }

                    if ([3, 4, 5, 6].includes(Number(this.form.format))) {
                        if (!this.form.virtualUrl.trim()) {
                            alert("Webinar/Webcast URL is required.");
                            return;
                        }
                    }

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

    /* Modal container */
    .modal {
        background: #f9fafb;
        padding: 28px 32px 24px;
        border-radius: 24px;
        width: 1120px;
        max-width: 100%;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 24px 60px rgba(15, 23, 42, 0.25), 0 0 0 1px rgba(148, 163, 184, 0.35);
        font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
        animation: fadeIn 0.28s ease-out;
    }

    /* Header */
    .modal-header {
        display: flex;
        align-items: flex-start;
        justify-content: space-between;
        gap: 16px;
        margin-bottom: 20px;
    }

        .modal-header h2 {
            font-size: 24px;
            font-weight: 700;
            color: #0f172a;
            letter-spacing: -0.02em;
            margin: 0 0 4px;
        }

    .modal-subtitle {
        margin: 0;
        font-size: 13px;
        color: #64748b;
    }

    .modal-badge {
        align-self: center;
        padding: 6px 12px;
        font-size: 11px;
        font-weight: 600;
        text-transform: uppercase;
        letter-spacing: 0.08em;
        color: #0369a1;
        background: rgba(219, 234, 254, 0.9);
        border-radius: 999px;
        border: 1px solid rgba(129, 140, 248, 0.4);
    }

    /* Sections */
    .section-card {
        background: #ffffff;
        border-radius: 18px;
        padding: 18px 18px 14px;
        margin-bottom: 18px;
        border: 1px solid #e5e7eb;
        box-shadow: 0 12px 30px rgba(15, 23, 42, 0.06), 0 0 0 1px rgba(148, 163, 184, 0.08);
    }

    .section-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        margin-bottom: 14px;
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
        margin: 3px 0 0;
        font-size: 12px;
        color: #6b7280;
    }

    /* Layout */
    .form-container {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
    }

    .form-column {
        flex: 1 1 48%;
        min-width: 0;
    }

    .form-column-full {
        flex: 1 1 100%;
        margin-top: 8px;
    }

    /* Form elements */
    .form-group {
        margin-bottom: 14px;
    }

    label {
        font-weight: 500;
        margin-bottom: 6px;
        display: block;
        color: #111827;
        font-size: 13px;
    }

    .required {
        color: #dc2626;
        margin-left: 2px;
    }

    input,
    select,
    textarea {
        width: 100%;
        padding: 9px 11px;
        font-size: 13px;
        border: 1px solid #d1d5db;
        border-radius: 10px;
        background-color: #f9fafb;
        transition: all 0.18s ease;
        box-sizing: border-box;
    }

        input:focus,
        select:focus,
        textarea:focus {
            border-color: #43285D;
            background-color: #ffffff;
            outline: none;
            box-shadow: 0 0 0 1px rgba(129, 140, 248, 0.55), 0 0 0 4px rgba(129, 140, 248, 0.16);
        }

    textarea {
        resize: vertical;
        min-height: 90px;
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
        border: 1px dashed #d4d4d8;
        padding: 14px 14px 10px;
        margin-bottom: 12px;
        border-radius: 14px;
        background: #f9fafb;
        box-shadow: 0 4px 12px rgba(15, 23, 42, 0.04);
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

    .session-empty {
        display: flex;
        justify-content: flex-start;
        margin-top: 4px;
    }

    /* Checkbox / radio styling */
    input[type="checkbox"],
    input[type="radio"] {
        accent-color: #43285D;
        cursor: pointer;
    }

    .checkbox-row {
        display: flex;
        align-items: center;
        gap: 8px;
    }

        .checkbox-row input[type="checkbox"] {
            width: 16px;
            height: 16px;
        }

        .checkbox-row label {
            margin: 0;
        }

    .radio-row {
        display: flex;
        align-items: center;
        gap: 16px;
    }

    .radio-item {
        display: inline-flex;
        align-items: center;
        gap: 6px;
    }

    /* Multi-session toggle label */
    .toggle-label {
        display: inline-flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        color: #374151;
    }

        .toggle-label input[type="checkbox"] {
            width: 18px;
            height: 18px;
        }

    /* Buttons */
    .button-group {
        display: flex;
        justify-content: flex-end;
        gap: 10px;
        margin-top: 18px;
        padding-top: 12px;
        border-top: 1px solid #e5e7eb;
        position: sticky;
        bottom: 0;
        background: linear-gradient(to top, #f9fafb 70%, rgba(249, 250, 251, 0.7));
    }

    .btn-primary {
        background: linear-gradient(135deg, #43285D, #5A3A7D);
        color: white;
        padding: 10px 24px;
        border: none;
        font-size: 14px;
        border-radius: 999px;
        font-weight: 600;
        cursor: pointer;
        transition: all 0.18s ease;
        box-shadow: 0 6px 14px rgba(67, 40, 93, 0.35);
    }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 10px 22px rgba(67, 40, 93, 0.55);
            background: linear-gradient(135deg, #4D316E, #6A4796); /* lighter hover */
        }

    .btn-secondary {
        background-color: #e5e7eb;
        color: #111827;
        padding: 9px 18px;
        border: none;
        font-size: 13px;
        border-radius: 999px;
        font-weight: 500;
        cursor: pointer;
        transition: background-color 0.12s ease, transform 0.12s ease;
    }

        .btn-secondary:hover {
            background-color: #d1d5db;
            transform: translateY(-0.5px);
        }

    .btn-danger {
        background-color: #fee2e2;
        color: #b91c1c;
    }

    /* Animations */
    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(8px) scale(0.99);
        }

        to {
            opacity: 1;
            transform: translateY(0) scale(1);
        }
    }

    /* Responsive */
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
    .modal-close {
        position: absolute;
        top: 14px;
        right: 18px;
        background: rgba(255, 255, 255, 0.9);
        border: none;
        width: 32px;
        height: 32px;
        border-radius: 50%;
        font-size: 20px;
        font-weight: bold;
        cursor: pointer;
        color: #333;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 2px 6px rgba(0,0,0,0.15);
        transition: all 0.2s ease;
        z-index: 10;
    }

        .modal-close:hover {
            background: #ffe6e6;
            color: #c62828;
            transform: scale(1.05);
        }

    .modal-header-purple {
        background: linear-gradient(135deg, #43285D, #5A3A7D);
        padding: 26px 32px;
        margin: -28px -32px 24px -32px;
        border-top-left-radius: 24px;
        border-top-right-radius: 24px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        gap: 20px;
        box-shadow: inset 0 -1px 0 rgba(255,255,255,0.10);
    }

        /* Left side */
        .modal-header-purple h2 {
            color: white;
            font-size: 22px;
            font-weight: 700;
            margin: 0;
            letter-spacing: -0.3px;
        }

        .modal-header-purple p {
            color: #E6DBF5; /* softer lavender */
            margin: 4px 0 0 0;
            font-size: 13px;
        }

    /* Badge */
    .modal-badge {
        background: rgba(255, 255, 255, 0.15);
        color: white;
        padding: 8px 16px;
        border-radius: 999px;
        font-size: 11px;
        font-weight: 600;
        border: 1px solid rgba(255,255,255,0.25);
        white-space: nowrap;
    }

</style>