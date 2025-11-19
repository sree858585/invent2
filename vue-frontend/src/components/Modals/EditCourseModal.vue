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
                                    <option v-for="center in lookupData.trainingCenters.$values"
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
                                    <option v-for="region in lookupData.regions.$values"
                                            :key="region.code"
                                            :value="String(region.code)">
                                        {{ region.value }}
                                    </option>
                                </select>
                            </div>

                            <div class="form-group">
                                <label>Category *</label>
                                <select v-model="form.category" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="category in lookupData.categories.$values"
                                            :key="category.code"
                                            :value="String(category.code)">
                                        {{ category.value }}
                                    </option>
                                </select>
                            </div>
                        </div>

                        <div class="form-column">
                            <div class="form-group">
                                <label>Course Title *</label>
                                <select v-model="form.courseTitle" required>
                                    <option value="">-- Select --</option>
                                    <option v-for="subject in filteredSubjects"
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
                                    <option v-for="format in lookupData.formats.$values"
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

<script>import apiClient from '@/axios.js';

    export default {
        props: ['course'],
        data() {
            return {
                originalCourse: null,
                form: {
                    trainingCenter: '',
                    region: '',
                    category: '',
                    courseTitle: '',
                    instructor1: '',
                    instructor2: '',
                    startDate: '',
                    endDate: '',
                    startTime: '',
                    endTime: '',
                    regDeadline: '',
                    maxSeats: '',
                    trainingLocation: '',
                    virtualUrl: '',
                    deliverables: '',
                    format: '',
                    fundingType: '',
                    hideCourse: false,
                    courseSchedule: '',
                    isMultiSession: false,
                    sessions: [
                        {
                            date: '',
                            startTime: '',
                            endTime: '',
                            url: '',
                            trainingLocation: ''
                        }
                    ]
                },
                lookupData: {
                    trainingCenters: [],
                    regions: [],
                    categories: [],
                    subjects: [],
                    instructors: [],
                    deliverables: [],
                    formats: []
                },
                filteredSubjects: []
            };
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
                console.error('❌ Failed to load full course details', err);
                alert('Failed to load course details.');
            }
        },
        methods: {
            addSession() {
                if (this.form.sessions.length < 4) {
                    this.form.sessions.push({
                        date: '',
                        startTime: '',
                        endTime: '',
                        url: '',
                        trainingLocation: ''
                    });
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
                    console.error('Failed to load subjects by category:', err);
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
                    console.error('❌ Failed to fetch lookup data', err);
                }
            },
            async populateForm(course) {
                const c = course;

                const subject = this.lookupData.subjects.find(
                    (s) => s.subjectSysId === c.subjectSysId
                );
                const derivedCategory = subject ? String(subject.category) : '';

                this.form.trainingCenter = String(c.siteSysId);
                this.form.region = c.region ? String(c.region) : '';
                this.form.category = derivedCategory;
                await this.fetchSubjectsByCategory(derivedCategory);

                this.form.courseTitle = c.subjectSysId ? String(c.subjectSysId) : '';
                this.form.instructor1 = c.instructor1 ? String(c.instructor1) : '';
                this.form.instructor2 = c.instructor2 ? String(c.instructor2) : '';
                this.form.startDate = c.courseDate?.split('T')[0] || '';
                this.form.endDate = c.endDate?.split('T')[0] || '';
                this.form.startTime = c.courseTimeBegin?.substring(11, 16) || '';
                this.form.endTime = c.courseTimeEnd?.substring(11, 16) || '';
                this.form.regDeadline = c.regDeadLine?.split('T')[0] || '';
                this.form.maxSeats = c.maxSeats;
                this.form.trainingLocation = c.trainingLocation || '';
                this.form.virtualUrl = c.virtualUrl || '';
                this.form.deliverables = c.deliverable ? String(c.deliverable) : '';
                this.form.format = c.format ? String(c.format) : '';
                this.form.fundingType = c.rtc ? 'RTC' : c.coe ? 'COE' : 'Others';
                this.form.hideCourse = c.hidden;
                this.form.courseSchedule = c.information;
                this.form.isMultiSession = c.isMultiSession || false;

                const rawSessions = c.sessions?.$values || [];

                this.form.sessions = rawSessions.length
                    ? rawSessions.map((s) => ({
                        date: s.sessionDate?.split('T')[0] || '',
                        startTime: s.startTime?.substring(0, 5) || '',
                        endTime: s.endTime?.substring(0, 5) || '',
                        url: s.sessionUrl || '',
                        trainingLocation: s.trainingLocation ?? ''
                    }))
                    : [
                        {
                            date: '',
                            startTime: '',
                            endTime: '',
                            url: '',
                            trainingLocation: ''
                        }
                    ];
            },
            async submitUpdate() {
                try {
                    const n = (v) =>
                        v === '' || v === null || v === undefined ? null : Number(v);

                    const coursePayload = {
                        courseSysId: this.originalCourse.courseSysId,
                        siteSysId: n(this.form.trainingCenter),
                        region: n(this.form.region),
                        subjectSysId: n(this.form.courseTitle),
                        instructor1: n(this.form.instructor1),
                        instructor2: n(this.form.instructor2),

                        courseDate: this.form.startDate
                            ? new Date(this.form.startDate).toISOString()
                            : null,
                        endDate: this.form.endDate
                            ? new Date(this.form.endDate).toISOString()
                            : null,

                        courseTimeBegin: this.form.startTime
                            ? new Date(
                                `${this.form.startDate}T${this.form.startTime}:00`
                            ).toISOString()
                            : null,
                        courseTimeEnd: this.form.endTime
                            ? new Date(
                                `${this.form.endDate}T${this.form.endTime}:00`
                            ).toISOString()
                            : null,

                        regDeadLine: this.form.regDeadline
                            ? new Date(this.form.regDeadline).toISOString()
                            : null,

                        maxSeats: n(this.form.maxSeats),
                        trainingLocation: this.form.trainingLocation || null,
                        virtualUrl: this.form.virtualUrl || null,
                        deliverable: n(this.form.deliverables),
                        format: n(this.form.format),

                        rtc: this.form.fundingType === 'RTC',
                        coe: this.form.fundingType === 'COE',
                        otherFund: this.form.fundingType === 'Others',
                        hidden: !!this.form.hideCourse,

                        information: this.form.courseSchedule || null,
                        isMultiSession: !!this.form.isMultiSession,
                        dateModified: new Date().toISOString()
                    };

                    const sessions = this.form.isMultiSession
                        ? this.form.sessions
                            .filter((s) => s.date && s.startTime && s.endTime)
                            .map((s) => ({
                                sessionDate: new Date(s.date).toISOString(),
                                startTime: s.startTime,
                                endTime: s.endTime,
                                sessionUrl: s.url ?? '',
                                trainingLocation: s.trainingLocation ?? ''
                            }))
                        : [];

                    const requestPayload = { course: coursePayload, sessions };

                    const courseSysId = this.originalCourse.courseSysId;
                    await apiClient.put(`/CourseAdmin/update/${courseSysId}`, requestPayload, {
                        headers: { 'Content-Type': 'application/json' }
                    });

                    alert('Course updated successfully!');
                    this.$emit('updated');
                    this.$emit('close');
                } catch (err) {
                    console.error('Error updating course:', err?.response?.data || err);
                    alert('Failed to update course.');
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
</style>