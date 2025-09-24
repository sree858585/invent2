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
                             class="session-group"
                             style="border: 1px solid #ddd; padding: 16px; border-radius: 10px; margin-bottom: 16px; background-color: #fafafa">
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

                            <div class="form-group" style="align-self: flex-end" v-if="form.sessions.length > 1">
                                <button type="button" class="btn-secondary" @click="removeSession(index)">❌ Remove</button>
                            </div>

                            <div class="form-group" v-if="index === form.sessions.length - 1 && form.sessions.length < 4">
                                <button type="button" class="btn-secondary" @click="addSession">
                                    ➕ Add Session
                                </button>
                            </div>
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
                originalCourse: null,
                form: {
                    trainingCenter: '', region: '', category: '', courseTitle: '', instructor1: '',
                    instructor2: '', startDate: '', endDate: '', startTime: '', endTime: '',
                    regDeadline: '', maxSeats: '', trainingLocation: '', deliverables: '',
                    format: '', fundingType: '', hideCourse: false, courseSchedule: '',
                    isMultiSession: false,
                    sessions: [{ date: '', startTime: '', endTime: '', url: '' }]
                },
                lookupData: {
                    trainingCenters: [], regions: [], categories: [], subjects: [], instructors: [], deliverables: [], formats: []
                },
                filteredSubjects: []
            }
        },
        async mounted() {
            await this.fetchLookupData();
            const courseId = this.course.courseSysId;
            try {
                const res = await apiClient.get(`/CourseAdmin/courseWithSessions/${courseId}`);
                const courseWithSessions = res.data;
                this.originalCourse = courseWithSessions;
                this.populateForm(courseWithSessions);
            } catch (err) {
                console.error("❌ Failed to load full course details", err);
                alert("Failed to load course details.");
            }
        },
        methods: {
            addSession() {
                if (this.form.sessions.length < 4) {
                    this.form.sessions.push({ date: '', startTime: '', endTime: '', url: '' });
                }
            },
            removeSession(index) {
                if (this.form.sessions.length > 1) {
                    this.form.sessions.splice(index, 1);
                }
            },
            async fetchSubjectsByCategory(categoryCode) {
                try {
                    const res = await apiClient.get(`/CreateCourse/subjectsByCategory/${categoryCode}`);
                    this.filteredSubjects = res.data?.$values || res.data || [];
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
                } catch (err) {
                    console.error("❌ Failed to fetch lookup data", err);
                }
            },
            async populateForm(course) {
                const c = course;
                const subject = this.lookupData.subjects.find(s => s.subjectSysId === c.subjectSysId);
                const derivedCategory = subject ? String(subject.category) : '';

                this.form.trainingCenter = String(c.siteSysId);
                this.form.region = c.region ? String(c.region) : '';
                this.form.category = derivedCategory;
                await this.fetchSubjectsByCategory(derivedCategory);

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
                this.form.isMultiSession = c.isMultiSession || false;
                const rawSessions = c.sessions?.$values || [];

                this.form.sessions = rawSessions.length
                    ? rawSessions.map(s => ({
                        date: s.sessionDate?.split("T")[0] || '',
                        startTime: s.startTime?.substring(0, 5) || '',
                        endTime: s.endTime?.substring(0, 5) || '',
                        url: s.sessionUrl || ''
                    }))
                    : [{ date: '', startTime: '', endTime: '', url: '' }];
            },
            async submitUpdate() {
              try {
                // coerce numbers
                const n = v => (v === '' || v === null || v === undefined ? null : Number(v));

                const coursePayload = {
                  ...this.originalCourse,
                  siteSysId: n(this.form.trainingCenter),
                  region: n(this.form.region),
                  subjectSysId: n(this.form.courseTitle),
                  instructor1: n(this.form.instructor1),
                  instructor2: n(this.form.instructor2),

                  // dates: send ISO (entity has DateTime)
                  courseDate: this.form.startDate ? new Date(this.form.startDate).toISOString() : null,
                  endDate:    this.form.endDate   ? new Date(this.form.endDate).toISOString()   : null,

                  // begin/end: your entity uses DateTime – keep ISO composed from date + time
                  courseTimeBegin: (this.form.startDate && this.form.startTime)
                    ? new Date(`${this.form.startDate}T${this.form.startTime}:00`).toISOString()
                    : null,
                  courseTimeEnd: (this.form.endDate && this.form.endTime)
                    ? new Date(`${this.form.endDate}T${this.form.endTime}:00`).toISOString()
                    : null,

                  regDeadLine: this.form.regDeadline ? new Date(this.form.regDeadline).toISOString() : null,

                  maxSeats: n(this.form.maxSeats),
                  trainingLocation: this.form.trainingLocation || null,
                  deliverable: n(this.form.deliverables),
                  format: n(this.form.format),

                  // booleans
                  rtc: this.form.fundingType === 'RTC',
                  coe: this.form.fundingType === 'COE',
                  otherFund: this.form.fundingType === 'Others',
                  hidden: !!this.form.hideCourse,

                  information: this.form.courseSchedule || null,
                  isMultiSession: !!this.form.isMultiSession,
                  dateModified: new Date().toISOString()
                };

                // sessions: API expects { sessionDate: DateTime, startTime: "HH:mm", endTime: "HH:mm", sessionUrl }
                const sessions = this.form.isMultiSession
                  ? this.form.sessions
                      .filter(s => s.date && s.startTime && s.endTime)
                      .map(s => ({
                        sessionDate: new Date(s.date).toISOString(),
                        startTime: s.startTime,           // "HH:mm"
                        endTime:   s.endTime,             // "HH:mm"
                        sessionUrl: s.url || null
                      }))
                  : [];

                const requestPayload = { course: coursePayload, sessions };

                const courseSysId = this.originalCourse.courseSysId;
                await apiClient.put(`/CourseAdmin/update/${courseSysId}`, requestPayload, {
                  headers: { 'Content-Type': 'application/json' }
                });

                alert("Course updated successfully!");
                this.$emit("updated");
                this.$emit("close");
              } catch (err) {
                console.error("Error updating course:", err?.response?.data || err);
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
</style>