<template>
    <div class="admin-roles-container">
        <div class="header">
            <h2>👔 Manager Role Management</h2>
        </div>

        <div class="filter-panel">
            <input v-model="filters.lastName" placeholder="🔍 Search by Last Name" @input="fetchUsers" />
            <input v-model="filters.email" placeholder="✉️ Search by Email" @input="fetchUsers" />
        </div>

        <table class="modern-table" v-if="users.length > 0">
            <thead>
                <tr>
                    <th>Full Name</th>
                    <th>Email</th>
                    <th>Status</th>
                    <th>Manager Access</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="user in users" :key="user.userId">
                    <td>{{ user.firstName }} {{ user.lastName }}</td>
                    <td>{{ user.email }}</td>
                    <td>
                        <span :class="user.isAdmin ? 'badge-admin' : user.isManager ? 'badge-manager' : 'badge-user'">
                            {{ user.isAdmin ? 'Admin' : user.isManager ? 'Manager' : 'User' }}
                        </span>
                    </td>
                    <td>
                        <button :class="['btn-action', user.isManager ? 'remove' : 'add']"
                                @click="openConfirmation(user)">
                            {{ user.isManager ? 'Remove' : 'Make Manager' }}
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>

        <p v-if="users.length === 0" class="empty-message">No users found.</p>

        <div class="pagination" v-if="totalPages > 1">
            <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">Prev</button>
            <span>Page {{ currentPage }} of {{ totalPages }}</span>
            <button @click="changePage(currentPage + 1)" :disabled="currentPage >= totalPages">Next</button>
        </div>

        <AccessConfirmModal v-if="showConfirmModal"
                            :email="selectedUser?.email"
                            :isGranting="!selectedUser?.isManager"
                            roleType="Manager"
                            @close="showConfirmModal = false"
                            @confirm="confirmToggle" />

        <AccessSuccessModal v-if="showSuccessModal"
                            :user="selectedUser"
                            :action="selectedUser?.isManager ? 'removed from' : 'granted'"
                            roleType="Manager"
                            @close="showSuccessModal = false" />
    </div>
</template>

<script>import apiClient from '@/axios.js';
    import AccessConfirmModal from '@/components/Modals/AccessConfirmModal.vue';
    import AccessSuccessModal from '@/components/Modals/AccessSuccessModal.vue';

    export default {
        name: 'ManagerRoles',
        components: {
            AccessConfirmModal,
            AccessSuccessModal
        },
        data() {
            return {
                users: [],
                totalUsers: 0,
                currentPage: 1,
                pageSize: 10,
                filters: {
                    lastName: '',
                    email: ''
                },
                showConfirmModal: false,
                showSuccessModal: false,
                selectedUser: null
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.totalUsers / this.pageSize);
            }
        },
        mounted() {
            this.fetchUsers();
        },
        methods: {
            async fetchUsers() {
                const params = {
                    lastName: this.filters.lastName,
                    email: this.filters.email,
                    page: this.currentPage,
                    pageSize: this.pageSize
                };
                try {
                    const res = await apiClient.get('/RoleManagement/managers', { params });
                    this.users = res.data?.data?.$values ?? [];
                    this.totalUsers = res.data?.total || 0;
                } catch (err) {
                    console.error('Error fetching users:', err);
                }
            },
            openConfirmation(user) {
                this.selectedUser = user;
                this.showConfirmModal = true;
            },
            async confirmToggle() {
                if (!this.selectedUser) return;
                const url = this.selectedUser.isManager
                    ? '/RoleManagement/remove-manager'
                    : '/RoleManagement/assign-manager';
                try {
                    await apiClient.post(url, this.selectedUser.userId);
                    this.showConfirmModal = false;
                    this.showSuccessModal = true;
                    await this.fetchUsers();
                } catch (err) {
                    console.error('Failed to update manager access:', err);
                    this.showConfirmModal = false;
                }
            },
            changePage(page) {
                if (page < 1 || page > this.totalPages) return;
                this.currentPage = page;
                this.fetchUsers();
            }
        }
    };</script>


<style scoped>
    .admin-roles-container {
        padding: 24px 40px;
        font-family: 'Segoe UI', sans-serif;
        color: #333;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 24px;
    }

        .header h2 {
            font-size: 26px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .filter-panel {
        display: flex;
        gap: 16px;
        margin-bottom: 20px;
    }

        .filter-panel input {
            padding: 10px;
            border-radius: 10px;
            border: 1px solid #ccc;
            flex: 1;
            font-size: 14px;
        }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        background-color: #fff;
        border-radius: 10px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
        overflow: hidden;
    }

        .modern-table th,
        .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left;
        }

        .modern-table th {
            background-color: #f9fafb;
            font-weight: 600;
        }

        .modern-table tbody tr:hover {
            background-color: #f5f5f5;
        }

    /* Badges */
    .badge-admin {
        background-color: #e0f7e9;
        color: #2e7d32;
        padding: 4px 10px;
        border-radius: 12px;
        font-size: 13px;
        font-weight: 600;
    }

    .badge-user {
        background-color: #ffebee;
        color: #c62828;
        padding: 4px 10px;
        border-radius: 12px;
        font-size: 13px;
        font-weight: 600;
    }

    /* Buttons */
    .btn-action {
        padding: 6px 14px;
        border-radius: 6px;
        font-weight: bold;
        border: none;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-action.add {
            background-color: #4caf50;
            color: white;
        }

        .btn-action.remove {
            background-color: #f44336;
            color: white;
        }

        .btn-action:hover {
            opacity: 0.85;
        }

    /* Pagination */
    .pagination {
        margin-top: 20px;
        text-align: center;
    }

        .pagination button {
            background: #f1f1f1;
            border: 1px solid #ccc;
            padding: 6px 12px;
            margin: 0 4px;
            border-radius: 6px;
            cursor: pointer;
        }

            .pagination button:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }

    .empty-message {
        text-align: center;
        color: #888;
        margin-top: 30px;
        font-style: italic;
    }
    .badge-manager {
        background-color: #e8f4fd;
        color: #1976d2;
        padding: 4px 10px;
        border-radius: 12px;
        font-size: 13px;
        font-weight: 600;
    }
</style>
