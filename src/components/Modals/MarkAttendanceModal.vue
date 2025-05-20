<template>
    <div class="modal-overlay">
        <div class="modal">
            <button class="close-btn" @click="$emit('close')">×</button>
            <h3>📋 Mark Attendance</h3>
            <p><strong>Course:</strong> {{ course?.subjectTitle }}</p>

            <div class="search-fields">
                <input v-model="lastName" placeholder="Search by Last Name" @input="fetchUsers" />
                <input v-model="email" placeholder="Search by Email" @input="fetchUsers" />
            </div>

            <!-- Add this after search-fields -->
            <div class="mark-all-container">
                <div class="attendance-summary">
                    <span><strong>Registered:</strong> {{ globalRegistered }}</span>
                    <span><strong>Attended:</strong> {{ globalAttended }}</span>
                    <span><strong>Not Attended:</strong> {{ globalNotAttended }}</span>
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

            <p v-if="!users.length">No registered users found.</p>

            <button class="btn-secondary" @click="$emit('close')">Close</button>
        </div>
    </div>
</template>

<script>import { ref, onMounted, computed } from 'vue';
    import apiClient from '@/axios.js';

    export default {
        props: ['course'],
        setup(props) {
            const users = ref([]);
            const lastName = ref('');
            const email = ref('');
            const page = ref(1);
            const pageSize = ref(20);
            const totalUsers = ref(0);
            const pageSizeOptions = [20, 30, 50, 90, 200, 1000, 10000];
            const attendedCount = computed(() => users.value.filter(u => u.attended).length);
            const notAttendedCount = computed(() => users.value.filter(u => !u.attended).length);
            const globalRegistered = ref(0);
            const globalAttended = ref(0);
            const globalNotAttended = ref(0);
            const fetchAttendanceSummary = async () => {
                try {
                    const res = await apiClient.get('/Attendance/summary', {
                        params: { courseId: props.course.courseSysId }
                    });
                    globalRegistered.value = res.data.registered;
                    globalAttended.value = res.data.attended;
                    globalNotAttended.value = res.data.notAttended;
                } catch (err) {
                    console.error('❌ Failed to load attendance summary:', err);
                }
            };
            const allAttended = computed(() => users.value.every(u => u.attended));

            const toggleMarkAll = async () => {
                const shouldAttend = !allAttended.value;

                for (const user of users.value) {
                    if (user.attended !== shouldAttend) {
                        user.attended = shouldAttend;
                        await toggleAttendance(user); 
                    }
                }
            };

            const fetchUsers = async () => {
                const params = {
                    courseId: props.course.courseSysId,
                    lastName: lastName.value,
                    email: email.value,
                    page: page.value,
                    pageSize: pageSize.value
                };

                try {
                    const res = await apiClient.get('/CourseAdmin/registered-users', { params });
                    users.value = res.data.data?.$values?.map(u => ({
                        ...u,
                        attended: u.attended ?? false
                    })) ?? [];
                    totalUsers.value = res.data.total || 0;
                } catch (err) {
                    console.error('❌ Error loading registered users:', err);
                }
            };

            const toggleAttendance = async (user) => {
                try {
                    await apiClient.put(`/Attendance/mark`, {
                        userSysId: user.userSysId,
                        courseSysId: props.course.courseSysId,
                        attended: user.attended
                    });

                    await fetchAttendanceSummary(); 
                } catch (err) {
                    console.error('❌ Failed to update attendance:', err);
                }
            };

            onMounted(() => {
                fetchUsers();
                fetchAttendanceSummary();
            });
            return {
                users,
                lastName,
                email,
                page,
                pageSize,
                pageSizeOptions,
                totalUsers,
                fetchUsers,
                toggleAttendance,
                toggleMarkAll,
                allAttended,
                attendedCount,
                notAttendedCount,
                globalRegistered,      
                globalAttended,        
                globalNotAttended       
            };
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
    }

    .search-fields {
        display: flex;
        gap: 12px;
        margin-bottom: 24px;
    }

    .user-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 12px;
    }

        .user-table th,
        .user-table td {
            padding: 12px 16px;
            border-bottom: 1px solid #e0e0e0;
        }

    .btn-green {
        background-color: #4CAF50;
        color: white;
        padding: 8px 16px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        margin-bottom: 12px;
        cursor: pointer;
    }

    .btn-secondary {
        background-color: #ccc;
        color: #333;
        padding: 10px 16px;
        border-radius: 8px;
        border: none;
        font-size: 14px;
        cursor: pointer;
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
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: #ccc;
        transition: .4s;
        border-radius: 26px;
    }

        .slider:before {
            position: absolute;
            content: "";
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
    .search-fields input {
        flex: 1;
        padding: 12px 16px;
        border: 1px solid #ccc;
        border-radius: 10px;
        font-size: 14px;
        background-color: #fdfdfd;
        transition: border 0.3s ease, box-shadow 0.3s ease;
    }

        .search-fields input:focus {
            border-color: #4CAF50;
            outline: none;
            box-shadow: 0 0 0 2px rgba(76, 175, 80, 0.2);
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

        .pagination-controls label {
            display: flex;
            align-items: center;
            gap: 8px;
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
                border-color: #4CAF50;
                outline: none;
                box-shadow: 0 0 0 2px rgba(76, 175, 80, 0.2);
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
        transition: color 0.2s ease;
    }

        .close-btn:hover {
            color: #333;
        }

    .mark-all-container {
        display: flex;
        justify-content: flex-end;
        margin-bottom: 12px;
    }
    .mark-all-container {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
        flex-wrap: wrap;
        gap: 12px;
    }

    .attendance-summary {
        display: flex;
        gap: 24px;
        font-size: 18px;
        color: #444;
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
</style>
