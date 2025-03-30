<template>
    <div class="modal-overlay">
        <div class="modal">
            <h2>Edit Course</h2>

            <form @submit.prevent="submitUpdate">
                <div class="form-container">
                    <div class="form-column">
                        <div class="form-group">
                            <label>Training Center *</label>
                            <select v-model="form.trainingCenter" required>
                                <option value="">-- Select --</option>
                                <option v-for="center in lookupData.trainingCenters.$values" :key="center.siteSysId" :value="String(center.siteSysId)">
                                    {{ center.siteName }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Region *</label>
                            <select v-model="form.region" required>
                                <option value="">-- Select --</option>
                                <option v-for="region in lookupData.regions.$values" :key="region.code" :value="String(region.code)">
                                    {{ region.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Category *</label>
                            <select v-model="form.category" required>
                                <option value="">-- Select --</option>
                                <option v-for="category in lookupData.categories.$values" :key="category.code" :value="String(category.code)">
                                    {{ category.value }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Course Title *</label>
                            <select v-model="form.courseTitle" required>
                                <option value="">-- Select --</option>
                                <option v-for="subject in filteredSubjects" :key="subject.subjectSysId" :value="String(subject.subjectSysId)">
                                    {{ subject.courseTitle }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>1st Instructor</label>
                            <select v-model="form.instructor1">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors.$values" :key="instructor.instructorSysId" :value="String(instructor.instructorSysId)">
                                    {{ instructor.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>2nd Instructor</label>
                            <select v-model="form.instructor2">
                                <option value="">-- Select --</option>
                                <option v-for="instructor in lookupData.instructors.$values" :key="instructor.instructorSysId" :value="String(instructor.instructorSysId)">
                                    {{ instructor.name }}
                                </option>
                            </select>
                        </div>

                        <div class="form-group">
                            <label>Registration Deadline *</label>
                            <input type="date" v-model="form.regDeadline" required />
                        </div>
                    </div>

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
                                <option v-for="format in lookupData.formats.$values" :key="format.code" :value="String(format.code)">
                                    {{ format.value }}
                                </option>
                            </select>
                        </div>
                    </div>
                </div>

                <div class="form-column-full">
                    <div class="form-group">
                        <label># of Deliverables *</label>
                        <select v-model="form.deliverables" required>
                            <option value="">-- Select --</option>
                            <option v-for="deliverable in lookupData.deliverables.$values" :key="deliverable.id" :value="String(deliverable.id)">
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

                <div class="button-group">
                    <button type="submit" class="btn-primary">Update</button>
                    <button type="button" class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </form>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios.js';

    export default {
        props: ['course'],
        data() {
            return {
                form: {
                    trainingCenter: '', region: '', category: '', courseTitle: '', instructor1: '',
                    instructor2: '', startDate: '', endDate: '', startTime: '', endTime: '',
                    regDeadline: '', maxSeats: '', trainingLocation: '', deliverables: '',
                    format: '', fundingType: '', hideCourse: false, courseSchedule: ''
                },
                lookupData: {
                    trainingCenters: [], regions: [], categories: [], subjects: [], instructors: [], deliverables: [], formats: []
                },
                filteredSubjects: [] 

            }
        },
        async mounted() {
            await this.fetchLookupData();

            if (this.course) {
                this.populateForm();
            }
        },
        methods: {
            
            async fetchSubjectsByCategory(categoryCode) {
                try {
                    const res = await apiClient.get(`/CreateCourse/subjectsByCategory/${categoryCode}`);
                    // 🔥 FIX: If the response has $values, extract it:
                    this.filteredSubjects = res.data?.$values || res.data || [];
                    console.log("✅ Filtered Subjects Set:", this.filteredSubjects);
                } catch (err) {
                    console.error("Failed to load subjects by category:", err);
                    this.filteredSubjects = [];
                }
            },
            async fetchLookupData() {
                try {
                    const res = await apiClient.get('/CreateCourse/lookup');
                    this.lookupData = {
                        trainingCenters: res.data.trainingCenters || [],
                        regions: res.data.regions || [],
                        categories: res.data.categories || [],
                        subjects: res.data.subjects?.$values || res.data.subjects || [],
                        instructors: res.data.instructors || [],
                        deliverables: res.data.deliverables || [],
                        formats: res.data.formats || []
                    };
                    console.log("Subjects raw from backend:", res.data.subjects);

                    this.populateForm();

                } catch (err) {
                    console.error("❌ Failed to fetch lookup data", err);
                }
            },
            async populateForm() {
                const c = this.course;

                const subject = this.lookupData.subjects.find(s => s.subjectSysId === c.subjectSysId);
                const derivedCategory = subject ? String(subject.category) : '';

                this.form.trainingCenter = String(c.siteSysId);
                this.form.region = c.region ? String(c.region) : '';
                this.form.category = derivedCategory;

                // 👇 Await fetching subjects for the category before assigning course title
                await this.fetchSubjectsByCategory(derivedCategory);
                // Small delay to ensure DOM updates are completed before binding
                setTimeout(() => {
                    this.form.courseTitle = c.subjectSysId ? String(c.subjectSysId) : '';
                }, 0);
                this.form.courseTitle = c.subjectSysId ? String(c.subjectSysId) : '';

                this.form.instructor1 = c.instructor1 ? String(c.instructor1) : '';
                this.form.instructor2 = c.instructor2 ? String(c.instructor2) : '';
                this.form.startDate = c.courseDate?.split('T')[0];
                this.form.endDate = c.endDate?.split('T')[0];
                this.form.startTime = c.courseTimeBegin?.substring(11, 16);
                this.form.endTime = c.courseTimeEnd?.substring(11, 16);
                this.form.regDeadline = c.regDeadLine?.split('T')[0];
                this.form.maxSeats = c.maxSeats;
                this.form.trainingLocation = c.trainingLocation;
                this.form.deliverables = c.deliverable ? String(c.deliverable) : '';
                this.form.format = c.format ? String(c.format) : '';
                this.form.fundingType = c.rtc ? 'RTC' : c.coe ? 'COE' : 'Others';
                this.form.hideCourse = c.hidden;
                this.form.courseSchedule = c.information;
            },
            async submitUpdate() {
                try {
                    const payload = {
                        ...this.course,
                        siteSysId: this.form.trainingCenter,
                        region: this.form.region,
                        subjectSysId: this.form.courseTitle,
                        instructor1: this.form.instructor1,
                        instructor2: this.form.instructor2,
                        courseDate: new Date(this.form.startDate).toISOString(),
                        endDate: new Date(this.form.endDate).toISOString(),
                        courseTimeBegin: new Date(`${this.form.startDate}T${this.form.startTime}:00Z`).toISOString(),
                        courseTimeEnd: new Date(`${this.form.endDate}T${this.form.endTime}:00Z`).toISOString(),
                        regDeadLine: new Date(this.form.regDeadline).toISOString(),
                        maxSeats: this.form.maxSeats,
                        trainingLocation: this.form.trainingLocation,
                        deliverable: this.form.deliverables,
                        format: this.form.format,
                        rtc: this.form.fundingType === 'RTC',
                        coe: this.form.fundingType === 'COE',
                        otherFund: this.form.fundingType === 'Others',
                        hidden: this.form.hideCourse,
                        information: this.form.courseSchedule,
                        dateModified: new Date().toISOString()
                    };
                    const courseSysId = this.course.courseSysId;
                    await apiClient.put(`/CourseAdmin/update/${courseSysId}`, payload);
                    alert("Course updated successfully!");
                    this.$emit("updated"); //  Emit this
                    this.$emit("close");
                } catch (err) {
                    console.error("Error updating course:", err);
                    alert("Failed to update course.");
                }
            }
        },
        watch: {
            'form.category'(newCategory) {
                if (newCategory) {
                    this.fetchSubjectsByCategory(newCategory);
                } else {
                    this.filteredSubjects = [];
                }
            }
        },
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
</style>