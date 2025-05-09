<template>
    <div class="modal-overlay">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')">×</button>
            <h3>📊 View Attendance</h3>
            <p style="font-size: 18px; font-weight: bold; margin-bottom: 4px;">{{ course.subjectTitle }}</p>
            <div class="attendance-summary">
                <p><strong>Date:</strong> {{ formatDate(course.courseDate) }} - {{ formatDate(course.endDate || course.courseDate) }}</p>
                <p><strong>Instructor 1:</strong> {{ course.instructorLabel || 'N/A' }}</p>
                <p><strong>Instructor 2:</strong> {{ course.instructor2Label || 'N/A' }}</p>
                <p><strong>Registered:</strong> {{ summary.registered }}</p>
                <p><strong>Attended:</strong> {{ summary.attended }}</p>
                <p><strong>Absent:</strong> {{ summary.registered - summary.attended }}</p>
            </div>

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
                <span>
                    Showing {{ start + 1 }}–{{ end }} of {{ attendedUsers.length }}
                </span>

                <label>
                    Show
                    <select v-model="pageSize" @change="page = 1">
                        <option v-for="opt in pageSizeOptions" :key="opt" :value="opt">{{ opt }}</option>
                    </select>
                    per page
                </label>

                <div>
                    <button :disabled="page === 1" @click="page--">⏮ Prev</button>
                    <span>Page {{ page }}</span>
                    <button :disabled="end >= attendedUsers.length" @click="page++">Next ⏭</button>
                </div>
            </div>

            <button class="btn-secondary" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script setup>
    // Fix for eslint no-undef
/* global defineProps */

import { ref, onMounted, computed } from 'vue'
    import apiClient from '@/axios'
    import * as XLSX from 'xlsx'
    import jsPDF from 'jspdf'
    import autoTable from 'jspdf-autotable'

    const props = defineProps({ course: Object })

    const summary = ref({ registered: 0, attended: 0 })
    const attendedUsers = ref([])

    const page = ref(1)
    const pageSize = ref(15)
    const pageSizeOptions = [15, 50, 200, 500, 1000, 10000]

    const start = computed(() => (page.value - 1) * pageSize.value)
    const end = computed(() => Math.min(start.value + pageSize.value, attendedUsers.value.length))
    const pagedUsers = computed(() => attendedUsers.value.slice(start.value, end.value))

    const formatDate = date => new Date(date).toLocaleDateString('en-US')

    const fetchSummary = async () => {
        const res = await apiClient.get('/Attendance/summary', {
            params: { courseId: props.course.courseSysId }
        })
        summary.value = res.data
    }

    const fetchAttendedUsers = async () => {
        const res = await apiClient.get('/CourseAdmin/registered-users', {
            params: {
                courseId: props.course.courseSysId,
                page: 1,
                pageSize: 2000
            }
        })
        attendedUsers.value = res.data.data?.$values?.filter(u => u.attended) ?? []
    }

    const downloadExcel = () => {
        const headerData = [
            ["Course Title", props.course.subjectTitle],
            ["Date", `${formatDate(props.course.courseDate)} - ${formatDate(props.course.endDate || props.course.courseDate)}`],
            ["Instructor 1", props.course.instructorLabel || 'N/A'],
            ["Instructor 2", props.course.instructor2Label || 'N/A'],
            ["Registered", summary.value.registered],
            ["Attended", summary.value.attended],
            ["Absent", summary.value.registered - summary.value.attended],
            [],
            ["Full Name", "Email", "Title", "Organization"]
        ];

        const userData = attendedUsers.value.map(user => [
            user.fullName,
            user.email,
            user.title ?? '',
            user.organization ?? ''
        ]);

        const worksheet = XLSX.utils.aoa_to_sheet([...headerData, ...userData]);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, 'Attendance');
        XLSX.writeFile(workbook, `${props.course.subjectTitle}_Attendance.xlsx`);
    };

    

    const downloadPDF = () => {
        const doc = new jsPDF();
        doc.setFontSize(14);
        doc.text(`${props.course.subjectTitle} – Attendance Report`, 14, 16);

        doc.setFontSize(11);
        doc.text(`Date: ${formatDate(props.course.courseDate)} - ${formatDate(props.course.endDate || props.course.courseDate)}`, 14, 24);
        doc.text(`Instructor 1: ${props.course.instructorLabel || 'N/A'}`, 14, 31);
        doc.text(`Instructor 2: ${props.course.instructor2Label || 'N/A'}`, 14, 38);
        doc.text(`Registered: ${summary.value.registered}`, 14, 45);
        doc.text(`Attended: ${summary.value.attended}`, 14, 52);
        doc.text(`Absent: ${summary.value.registered - summary.value.attended}`, 14, 59);

        autoTable(doc, {
            head: [['Full Name', 'Email', 'Title', 'Organization']],
            body: attendedUsers.value.map(user => [
                user.fullName,
                user.email,
                user.title ?? '',
                user.organization ?? ''
            ]),
            startY: 66
        });

        doc.save(`${props.course.subjectTitle}_Attendance.pdf`);
    };

    onMounted(() => {
        fetchSummary()
        fetchAttendedUsers()
    })</script>

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
        background: #fff;
        padding: 36px;
        border-radius: 18px;
        width: 960px;
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
        font-family: 'Segoe UI', sans-serif;
    }

    .close-btn {
        position: absolute;
        top: 12px;
        right: 16px;
        background: none;
        border: none;
        font-size: 28px;
        color: #888;
        cursor: pointer;
    }

    .attendance-summary {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
        margin-bottom: 20px;
        font-size: 16px;
    }

    .user-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 16px;
    }

        .user-table th,
        .user-table td {
            padding: 12px 16px;
            border-bottom: 1px solid #e0e0e0;
        }

    .pagination-controls {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 14px;
        margin-top: 20px;
    }

    .btn-secondary {
        margin-top: 20px;
        background-color: #ccc;
        padding: 10px 16px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }
    .pagination-controls {
        display: flex;
        flex-wrap: wrap;
        justify-content: space-between;
        align-items: center;
        margin-top: 24px;
        padding-top: 16px;
        border-top: 1px solid #e0e0e0;
        font-size: 14px;
        gap: 16px;
    }

        .pagination-controls label {
            display: flex;
            align-items: center;
            gap: 8px;
            font-weight: 500;
        }

        .pagination-controls select {
            padding: 6px 12px;
            border-radius: 8px;
            border: 1px solid #ccc;
            background-color: #fff;
            font-size: 14px;
            transition: border 0.3s ease;
        }

            .pagination-controls select:focus {
                border-color: #1976d2;
                outline: none;
                box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.2);
            }

        .pagination-controls button {
            background-color: #f1f1f1;
            color: #333;
            border: 1px solid #ccc;
            padding: 6px 12px;
            border-radius: 8px;
            cursor: pointer;
            font-weight: 500;
            transition: background-color 0.3s ease;
        }

            .pagination-controls button:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }

            .pagination-controls button:hover:not(:disabled) {
                background-color: #e0e0e0;
            }
    .download-buttons {
        display: flex;
        gap: 12px;
        margin-bottom: 16px;
    }

    .btn-download {
        background-color: #1976d2;
        color: #fff;
        padding: 8px 14px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        cursor: pointer;
        transition: background-color 0.3s ease;
    }

        .btn-download:hover {
            background-color: #125ba5;
        }
    .user-table td:nth-child(3),
    .user-table td:nth-child(4) {
        max-width: 180px;
        word-wrap: break-word;
    }
</style>