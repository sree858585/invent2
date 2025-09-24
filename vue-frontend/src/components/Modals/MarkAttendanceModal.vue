<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')">×</button>

            <!-- Header -->
            <h3>🧾 Attendance</h3>
            <p class="course-title">{{ course?.subjectTitle }}</p>

            <!-- Always-visible summary -->
            <div class="attendance-summary">
                <p><strong>Date:</strong> {{ formatDate(course.courseDate) }} - {{ formatDate(course.endDate || course.courseDate) }}</p>
                <p><strong>Instructor 1:</strong> {{ course.instructorLabel || 'N/A' }}</p>
                <p><strong>Instructor 2:</strong> {{ course.instructor2Label || 'N/A' }}</p>
                <p><strong>Registered:</strong> {{ summary.registered }}</p>
                <p><strong>Attended:</strong> {{ summary.attended }}</p>
                <p><strong>Absent:</strong> {{ summary.registered - summary.attended }}</p>
            </div>

            <!-- Tabs -->
            <div class="tab-nav">
                <button :class="{ active: tab === 'mark' }" @click="switchTab('mark')">Mark</button>
                <button :class="{ active: tab === 'view' }" @click="switchTab('view')">View / Export</button>
            </div>

            <!-- ========== MARK TAB ========== -->
            <div v-if="tab === 'mark'">
                <div class="search-fields">
                    <input v-model="lastName" placeholder="Search by Last Name" @input="fetchUsers" />
                    <input v-model="email" placeholder="Search by Email" @input="fetchUsers" />
                </div>

                <div class="mark-all-container">
                    <div class="mark-stats">
                        <span><strong>Registered:</strong> {{ summary.registered }}</span>
                        <span><strong>Attended:</strong> {{ summary.attended }}</span>
                        <span><strong>Not Attended:</strong> {{ summary.registered - summary.attended }}</span>
                    </div>
                    <button class="btn-green" @click="toggleMarkAll">
                        {{ allAttended ? '❌ Unmark All' : '✔️ Mark All as Attended' }}
                    </button>
                </div>

                <table class="user-table" v-if="users.length > 0">
                    <thead>
                        <tr>
                            <th>Full Name</th>
                            <th>Email</th>
                            <th>Attend</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="user in users" :key="user.userSysId">
                            <td>{{ user.fullName }}</td>
                            <td>{{ user.email }}</td>
                            <td>
                                <label class="toggle-switch">
                                    <input type="checkbox" v-model="user.attended" @change="toggleAttendance(user)" />
                                    <span class="slider"></span>
                                </label>
                            </td>
                        </tr>
                    </tbody>
                </table>

                <p v-else>No registered users found.</p>

                <div class="pagination-controls" v-if="totalUsers > 0">
                    <span>
                        Showing {{ (page - 1) * pageSize + 1 }}–
                        {{ Math.min(page * pageSize, totalUsers) }} of {{ totalUsers }}
                    </span>

                    <label>
                        Show
                        <select v-model="pageSize" @change="page = 1; fetchUsers()">
                            <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
                        </select>
                        per page
                    </label>

                    <div>
                        <button :disabled="page === 1" @click="page-- && fetchUsers()">⏮ Prev</button>
                        <span>Page {{ page }}</span>
                        <button :disabled="page * pageSize >= totalUsers" @click="page++ && fetchUsers()">Next ⏭</button>
                    </div>
                </div>
            </div>

            <!-- ========== VIEW / EXPORT TAB ========== -->
            <div v-else>
                <div class="download-buttons" v-if="attendedUsers.length">
                    <button @click="downloadExcel" class="btn-download">⬇️ Download Excel</button>
                    <button @click="downloadPDF" class="btn-download">⬇️ Download PDF</button>
                </div>

                <table class="user-table" v-if="attendedUsers.length">
                    <thead>
                        <tr>
                            <th>Full Name</th>
                            <th>Email</th>
                            <th>Title</th>
                            <th>Organization</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="user in pagedUsers" :key="user.userSysId">
                            <td>{{ user.fullName }}</td>
                            <td>{{ user.email }}</td>
                            <td>{{ user.title || '—' }}</td>
                            <td>{{ user.organization || '—' }}</td>
                        </tr>
                    </tbody>
                </table>

                <p v-else>No attended users found.</p>

                <div class="pagination-controls" v-if="attendedUsers.length > 0">
                    <span>Showing {{ start + 1 }}–{{ end }} of {{ attendedUsers.length }}</span>

                    <label>
                        Show
                        <select v-model="viewPageSize" @change="viewPage = 1">
                            <option v-for="opt in viewPageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
                        </select>
                        per page
                    </label>

                    <div>
                        <button :disabled="viewPage === 1" @click="viewPage--">⏮ Prev</button>
                        <span>Page {{ viewPage }}</span>
                        <button :disabled="end >= attendedUsers.length" @click="viewPage++">Next ⏭</button>
                    </div>
                </div>
            </div>

            <button class="btn-secondary" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script setup>/* global defineProps */
    import { ref, onMounted, computed } from 'vue'
    import apiClient from '@/axios.js'
    import * as XLSX from 'xlsx'
    import jsPDF from 'jspdf'
    import autoTable from 'jspdf-autotable'

    const props = defineProps({ course: Object })

    // tabs
    const tab = ref('mark')

    // shared summary
    const summary = ref({ registered: 0, attended: 0 })

    // ------- MARK state -------
    const users = ref([])
    const lastName = ref('')
    const email = ref('')
    const page = ref(1)
    const pageSize = ref(20)
    const pageSizeOptions = [20, 30, 50, 90, 200, 1000, 10000]
    const totalUsers = ref(0)
    const allAttended = computed(() => users.value.length > 0 && users.value.every(u => u.attended))

    // ------- VIEW state -------
    const attendedUsers = ref([])
    const viewPage = ref(1)
    const viewPageSize = ref(15)
    const viewPageSizeOptions = [15, 50, 200, 500, 1000, 10000]
    const start = computed(() => (viewPage.value - 1) * viewPageSize.value)
    const end = computed(() => Math.min(start.value + viewPageSize.value, attendedUsers.value.length))
    const pagedUsers = computed(() => attendedUsers.value.slice(start.value, end.value))

    const formatDate = (date) => new Date(date).toLocaleDateString('en-US')

    // ---- API calls ----
    const fetchAttendanceSummary = async () => {
        try {
            const res = await apiClient.get('/Attendance/summary', {
                params: { courseId: props.course.courseSysId }
            })
            summary.value = res.data
        } catch (err) {
            console.error('❌ Failed to load attendance summary:', err)
        }
    }

    const fetchUsers = async () => {
        const params = {
            courseId: props.course.courseSysId,
            lastName: lastName.value,
            email: email.value,
            page: page.value,
            pageSize: pageSize.value
        }
        try {
            const res = await apiClient.get('/CourseAdmin/registered-users', { params })
            users.value = res.data.data?.$values?.map(u => ({ ...u, attended: u.attended ?? false })) ?? []
            totalUsers.value = res.data.total || 0
        } catch (err) {
            console.error('❌ Error loading registered users:', err)
        }
    }

    const fetchAttendedUsers = async () => {
        try {
            const res = await apiClient.get('/CourseAdmin/registered-users', {
                params: { courseId: props.course.courseSysId, page: 1, pageSize: 20000 }
            })
            attendedUsers.value = res.data.data?.$values?.filter(u => u.attended) ?? []
        } catch (err) {
            console.error('❌ Error loading attended users:', err)
        }
    }

    const toggleAttendance = async (user) => {
        try {
            await apiClient.put(`/Attendance/mark`, {
                userSysId: user.userSysId,
                courseSysId: props.course.courseSysId,
                attended: user.attended
            })
            await fetchAttendanceSummary()
            if (tab.value === 'view') await fetchAttendedUsers()
        } catch (err) {
            console.error('❌ Failed to update attendance:', err)
        }
    }

    const toggleMarkAll = async () => {
        const shouldAttend = !allAttended.value
        for (const user of users.value) {
            if (user.attended !== shouldAttend) {
                user.attended = shouldAttend
                await toggleAttendance(user)
            }
        }
        // keep view data fresh
        await fetchAttendedUsers()
    }

    // ---- Exporters ----
    const downloadExcel = () => {
        const headerData = [
            ['Course Title', props.course.subjectTitle],
            ['Date', `${formatDate(props.course.courseDate)} - ${formatDate(props.course.endDate || props.course.courseDate)}`],
            ['Instructor 1', props.course.instructorLabel || 'N/A'],
            ['Instructor 2', props.course.instructor2Label || 'N/A'],
            ['Registered', summary.value.registered],
            ['Attended', summary.value.attended],
            ['Absent', summary.value.registered - summary.value.attended],
            [],
            ['Full Name', 'Email', 'Title', 'Organization']
        ]
        const userData = attendedUsers.value.map(u => [u.fullName, u.email, u.title ?? '', u.organization ?? ''])
        const ws = XLSX.utils.aoa_to_sheet([...headerData, ...userData])
        const wb = XLSX.utils.book_new()
        XLSX.utils.book_append_sheet(wb, ws, 'Attendance')
        XLSX.writeFile(wb, `${props.course.subjectTitle}_Attendance.xlsx`)
    }

    const downloadPDF = () => {
        const doc = new jsPDF()
        doc.setFontSize(14)
        doc.text(`${props.course.subjectTitle} – Attendance Report`, 14, 16)

        doc.setFontSize(11)
        doc.text(`Date: ${formatDate(props.course.courseDate)} - ${formatDate(props.course.endDate || props.course.courseDate)}`, 14, 24)
        doc.text(`Instructor 1: ${props.course.instructorLabel || 'N/A'}`, 14, 31)
        doc.text(`Instructor 2: ${props.course.instructor2Label || 'N/A'}`, 14, 38)
        doc.text(`Registered: ${summary.value.registered}`, 14, 45)
        doc.text(`Attended: ${summary.value.attended}`, 14, 52)
        doc.text(`Absent: ${summary.value.registered - summary.value.attended}`, 14, 59)

        autoTable(doc, {
            head: [['Full Name', 'Email', 'Title', 'Organization']],
            body: attendedUsers.value.map(u => [u.fullName, u.email, u.title ?? '', u.organization ?? '']),
            startY: 66
        })
        doc.save(`${props.course.subjectTitle}_Attendance.pdf`)
    }

    // ---- Tabs ----
    const switchTab = async (t) => {
        tab.value = t
        if (t === 'mark') await fetchUsers()
        else await fetchAttendedUsers()
    }

    onMounted(async () => {
        await fetchAttendanceSummary()
        await fetchUsers()         // default first tab
    })</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        inset: 0;
        background-color: rgba(0,0,0,0.6);
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
        box-shadow: 0 20px 40px rgba(0,0,0,0.15);
        font-family: 'Segoe UI', sans-serif;
        position: relative;
    }

    .close-btn {
        position: absolute;
        top: 12px;
        right: 16px;
        background: none;
        border: none;
        font-size: 28px;
        font-weight: bold;
        color: #888;
        cursor: pointer;
    }

        .close-btn:hover {
            color: #333;
        }

    .course-title {
        font-size: 18px;
        font-weight: 700;
        margin: 6px 0 8px;
    }

    .attendance-summary {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
        margin-bottom: 16px;
        font-size: 16px;
    }

    .tab-nav {
        display: flex;
        gap: 8px;
        margin-bottom: 12px;
    }

        .tab-nav button {
            padding: 8px 14px;
            border: 1px solid #ccc;
            border-radius: 8px;
            background: #f6f7f9;
            cursor: pointer;
        }

            .tab-nav button.active {
                background: #1976d2;
                color: #fff;
                border-color: #1976d2;
            }

    .search-fields {
        display: flex;
        gap: 12px;
        margin-bottom: 16px;
    }

        .search-fields input {
            flex: 1;
            padding: 12px 16px;
            border: 1px solid #ccc;
            border-radius: 10px;
            font-size: 14px;
            background-color: #fdfdfd;
            transition: border .3s ease, box-shadow .3s ease;
        }

            .search-fields input:focus {
                border-color: #4CAF50;
                outline: none;
                box-shadow: 0 0 0 2px rgba(76,175,80,.2);
            }

    .mark-all-container {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        gap: 12px;
        flex-wrap: wrap;
    }

    .mark-stats {
        display: flex;
        gap: 24px;
        font-size: 16px;
        color: #444;
    }

    .btn-green {
        background-color: #4CAF50;
        color: white;
        padding: 8px 16px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        cursor: pointer;
    }

    .user-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 10px;
    }

        .user-table th, .user-table td {
            padding: 12px 16px;
            border-bottom: 1px solid #e0e0e0;
        }

    .toggle-switch {
        position: relative;
        display: inline-block;
        width: 50px;
        height: 26px;
    }

        .toggle-switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    .slider {
        position: absolute;
        cursor: pointer;
        inset: 0;
        background-color: #ccc;
        transition: .4s;
        border-radius: 26px;
    }

        .slider:before {
            content: "";
            position: absolute;
            height: 20px;
            width: 20px;
            left: 3px;
            bottom: 3px;
            background-color: white;
            transition: .4s;
            border-radius: 50%;
        }

    .toggle-switch input:checked + .slider {
        background-color: #4CAF50;
    }

        .toggle-switch input:checked + .slider:before {
            transform: translateX(24px);
        }

    .pagination-controls {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        align-items: center;
        margin-top: 20px;
        padding-top: 12px;
        border-top: 1px solid #e0e0e0;
        font-size: 14px;
        gap: 16px;
    }

        .pagination-controls select {
            padding: 6px 12px;
            border-radius: 8px;
            border: 1px solid #ccc;
            background-color: #fff;
        }

        .pagination-controls button {
            background-color: #f1f1f1;
            color: #333;
            border: 1px solid #ccc;
            padding: 6px 12px;
            border-radius: 8px;
            cursor: pointer;
        }

    .btn-secondary {
        margin-top: 16px;
        background-color: #ccc;
        color: #333;
        padding: 10px 16px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }

    .download-buttons {
        display: flex;
        gap: 12px;
        margin: 10px 0 14px;
    }

    .btn-download {
        background-color: #1976d2;
        color: #fff;
        padding: 8px 14px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        cursor: pointer;
    }

        .btn-download:hover {
            background-color: #125ba5;
        }

    .user-table td:nth-child(3), .user-table td:nth-child(4) {
        max-width: 180px;
        word-wrap: break-word;
    }
</style>