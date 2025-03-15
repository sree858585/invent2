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
                                <option v-for="center in lookupData.trainingCenters" :key="center.siteSysId" :value="center.siteSysId">
                                    {{ center.siteName }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Region *</label>
                            <select v-model="form.region" required>
                                <option value="">-- Select --</option>
                                <option v-for="region in lookupData.regions" :key="region.code" :value="region.code">
                                    {{ region.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Category *</label>
                            <select v-model="form.category" required>
                                <option value="">-- Select --</option>
                                <option v-for="category in lookupData.categories" :key="category.code" :value="category.code">
                                    {{ category.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Course Title *</label>
                            <select v-model="form.courseTitle" required>
                                <option value="">-- Select --</option>
                                <option v-for="subject in lookupData.subjects" :key="subject.subjectSysId" :value="subject.subjectSysId">
                                    {{ subject.courseTitle }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>1st Instructor</label>
                            <select v-model="form.instructor1">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors" :key="instructor.instructorSysId" :value="instructor.instructorSysId">
                                    {{ instructor.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>2nd Instructor</label>
                            <select v-model="form.instructor2">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors" :key="instructor.instructorSysId" :value="instructor.instructorSysId">
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
                                <option v-for="format in lookupData.formats" :key="format.code" :value="format.code">
                                    {{ format.value }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>

                <!-- Full-width Inputs -->
                <div class="form-column-full">
                    <div class="form-group">
                        <label># of Deliverables *</label>
                        <select v-model="form.deliverables" required>
                            <option value="">-- Select --</option>
                            <option v-for="deliverable in lookupData.deliverables" :key="deliverable.id" :value="deliverable.id">
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

                            <input type="radio" id="others" value="Others" v-model="form.fundingType" />
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
                    <button type="button" class="btn-secondary" @click="closeModal">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js"; // ✅ Fixed API Import

    export default {
        name: "ScheduleCourseModal",
        props: {
            isOpen: Boolean
        },
        emits: ["close", "submit"],
        data() {
            return {
                form: {
                    trainingCenter: "", region: "", category: "", courseTitle: "", instructor1: "",
                    instructor2: "", startDate: "", endDate: "", startTime: "", endTime: "",
                    regDeadline: "", maxSeats: "", trainingLocation: "", deliverables: "",
                    format: "", fundingType: "", hideCourse: false, courseSchedule: ""
                },
                lookupData: {
                    trainingCenters: [],
                    regions: [],
                    categories: [],
                    subjects: [],
                    instructors: [],
                    deliverables: [],
                    formats: []
                }
            };
        },
        watch: {
            isOpen(newVal) {
                if (newVal) {
                    this.fetchLookupData();
                }
            }
        },
        methods: {
            async fetchLookupData() {
                try {
                    console.log("Fetching lookup data...");
                    const response = await apiClient.get("/CreateCourse/lookup");
                    console.log("Lookup API Response:", response.data);

                    // Extract `$values` from `trainingCenters`
                    const trainingCenters = response.data.trainingCenters?.$values || [];

                    console.log("Extracted Training Centers:", trainingCenters); // ✅ Debugging

                    this.lookupData = {
                        trainingCenters: trainingCenters, // ✅ Ensure it's an array
                        regions: response.data.regions?.$values || [],
                        categories: response.data.categories?.$values || [],
                        subjects: response.data.subjects?.$values || [],
                        instructors: response.data.instructors?.$values || [],
                        deliverables: response.data.deliverables?.$values || [],
                        formats: response.data.formats?.$values || []
                    };

                    console.log("Final Training Centers in lookupData:", this.lookupData.trainingCenters);
                } catch (error) {
                    console.error("Error fetching lookup data:", error);
                }
            },
            closeModal() {
                this.$emit("close");
            },
            async submitCourse() {
                try {
                    // Ensure start and end times are combined with course date
                    const courseTimeBegin = this.form.startDate && this.form.startTime
                        ? new Date(`${this.form.startDate}T${this.form.startTime}:00Z`).toISOString()
                        : null;

                    const courseTimeEnd = this.form.endDate && this.form.endTime
                        ? new Date(`${this.form.endDate}T${this.form.endTime}:00Z`).toISOString()
                        : null;

                    const courseData = {
                        siteSysId: this.form.trainingCenter,
                        subjectSysId: this.form.courseTitle,
                        courseDate: this.form.startDate ? new Date(this.form.startDate).toISOString() : null,
                        endDate: this.form.endDate ? new Date(this.form.endDate).toISOString() : null,
                        courseTimeBegin: courseTimeBegin, // ✅ Fixed
                        courseTimeEnd: courseTimeEnd, // ✅ Fixed
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

                    console.log("Submitting Course Data:", courseData); // ✅ Debugging

                    const response = await apiClient.post("/CreateCourse/schedule", courseData);

                    console.log("Server Response:", response.data);
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
    /* Two-column Form Layout */
    .form-container {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
    }

    .form-column {
        flex: 1 1 45%;
    }

    .form-column-full {
        flex: 1 1 100%;
        margin-top: 20px;
    }

    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.8);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
    }

    .modal {
        background-color: #fff;
        padding: 40px;
        border-radius: 16px;
        width: 900px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.25);
    }

        .modal h2 {
            margin-bottom: 20px;
            font-size: 28px;
            text-align: center;
            color: #333;
        }

    .form-group {
        margin-bottom: 20px;
    }

    label {
        font-weight: bold;
        margin-bottom: 5px;
        display: block;
    }

    input,
    select {
        width: 100%;
        padding: 12px;
        border: 1px solid #ccc;
        border-radius: 6px;
        font-size: 14px;
        box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.1);
    }

    .button-group {
        display: flex;
        justify-content: space-between;
        margin-top: 30px;
    }

    .btn-primary {
        background-color: #3f51b5;
        color: white;
        padding: 10px 20px;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        width: 48%;
    }

    .btn-secondary {
        background-color: #f5f5f5;
        color: #333;
        padding: 10px 20px;
        border: 1px solid #ccc;
        border-radius: 6px;
        cursor: pointer;
        width: 48%;
    }
</style>
