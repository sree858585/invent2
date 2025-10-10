<template>
    <div class="roles-container">
        <div class="header">
            <h2>🛡️ Role Management</h2>
        </div>

        <div class="filter-panel">
            <input v-model="filters.name" placeholder="🔍 Search by Full Name" @input="debouncedFetch()" />
            <input v-model="filters.email" placeholder="✉️ Search by Email" @input="debouncedFetch()" />
        </div>
        <div v-if="loading" class="skeleton-wrap">
            <div class="skeleton-header">
                <div class="skeleton-pill w-32"></div>
                <div class="skeleton-pill w-48"></div>
            </div>
            <div class="skeleton-table">
                <div class="skeleton-row" v-for="n in 8" :key="n">
                    <span class="sk-cell w-56"></span>
                    <span class="sk-cell w-60"></span>
                    <span class="sk-cell w-40"></span>
                    <span class="sk-cell w-24"></span>
                    <span class="sk-cell w-28"></span>
                </div>
            </div>
            <p class="loading-text">Loading users…</p>
        </div>
        <table class="modern-table" v-else-if="users.length">
            <thead>
                <tr>
                    <th>Full Name</th>
                    <th>Email</th>
                    <th>Change Role</th>
                    <th>Locked</th>
                    <th style="width:220px;">
                        <div class="status-header">
                            <span>Status</span>
                            <select class="role-filter" v-model="filters.role">
                                <option value="All">All</option>
                                <option value="User">User</option>
                                <option value="Manager">Manager</option>
                                <option value="Admin">Admin</option>
                            </select>
                        </div>
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="u in users" :key="u.userId">
                    <td>{{ u.firstName }} {{ u.lastName }}</td>
                    <td>{{ u.email }}</td>
                    <td>
                        <select class="role-select"
                                :disabled="saving.has('role-'+u.userId)"
                                v-model="u.role"
                                @focus="stashPrevRole(u)"
                                @change="onRoleChange(u)">
                            <option value="User">User</option>
                            <option value="Manager">Manager</option>
                            <option value="Admin">Admin</option>
                        </select>
                    </td>
                    <td>
                        <label class="switch">
                            <input type="checkbox"
                                   :checked="u.isLocked"
                                   :disabled="saving.has('lock-'+u.userId)"
                                   @change="onToggleLock(u, $event.target.checked)" />
                            <span class="slider"></span>
                        </label>
                    </td>
                    <td>
                        <span class="badge" :class="'badge-' + u.role.toLowerCase()">{{ u.role }}</span>
                        <span v-if="u.isLocked" class="lock-pill">Locked</span>
                    </td>
                </tr>
            </tbody>
        </table>

        <p v-else class="empty-message">No users found.</p>

        <div class="pagination" v-if="totalPages > 1">
            <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">Prev</button>
            <span>Page {{ currentPage }} of {{ totalPages }}</span>
            <button @click="changePage(currentPage + 1)" :disabled="currentPage >= totalPages">Next</button>
        </div>

        <!-- Role Confirm -->
        <AccessConfirmModal v-if="showRoleConfirm"
                            :email="pendingUser?.email"
                            :isGranting="pendingNewRole !== 'User'"
                            :roleType="pendingNewRole"
                            @close="cancelRoleChange"
                            @confirm="confirmRoleChange" />

        <!-- Role Success -->
        <AccessSuccessModal v-if="showRoleSuccess"
                            :user="pendingUser"
                            :action="pendingNewRole === 'User' ? 'removed from' : 'assigned to'"
                            :roleType="pendingNewRole === 'User' ? (pendingPrevRole || 'Admin/Manager') : pendingNewRole"
                            @close="showRoleSuccess = false" />

        <!-- Lock Confirm -->
        <div v-if="showLockConfirm" class="modal-overlay">
            <div class="modal confirmation">
                <button class="close-btn" @click="cancelLockChange">&times;</button>
                <h3>⚠️ Confirm {{ willLock ? 'Lock' : 'Unlock' }}</h3>
                <p>
                    Are you sure you want to <strong>{{ willLock ? 'lock' : 'unlock' }}</strong>
                    the account for <strong>{{ pendingUser?.email }}</strong>?
                </p>
                <div class="button-group">
                    <button class="btn-confirm" @click="confirmLockChange">Yes, Confirm</button>
                    <button class="btn-cancel" @click="cancelLockChange">No, Cancel</button>
                </div>
            </div>
        </div>

        <!-- Lock Success -->
        <div v-if="showLockSuccess" class="modal-overlay">
            <div class="modal success">
                <h3>✅ Account {{ willLock ? 'Locked' : 'Unlocked' }}</h3>
                <p>
                    <strong>{{ pendingUser?.email }}</strong> has been {{ willLock ? 'locked' : 'unlocked' }}.
                </p>
                <div class="button-group">
                    <button class="btn-primary" @click="showLockSuccess = false">OK</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios.js';
    import AccessConfirmModal from '@/components/Modals/AccessConfirmModal.vue';
    import AccessSuccessModal from '@/components/Modals/AccessSuccessModal.vue';

    export default {
        name: 'RoleManagement',
        components: { AccessConfirmModal, AccessSuccessModal },
        data() {
            return {
                users: [],
                totalUsers: 0,
                currentPage: 1,
                pageSize: 10,
                filters: { name: '', email: '', role: 'All' },
                saving: new Set(),
                debounceTimer: null,
                loading: false,

                // role confirm state
                showRoleConfirm: false,
                showRoleSuccess: false,
                pendingUser: null,
                pendingPrevRole: null,
                pendingNewRole: null,

                // lock confirm state
                showLockConfirm: false,
                showLockSuccess: false,
                willLock: false
            };
        },
        computed: {
            totalPages() { return Math.ceil(this.totalUsers / this.pageSize); }
        },
        mounted() { this.fetchUsers(); },
        beforeUnmount() { if (this.debounceTimer) clearTimeout(this.debounceTimer); },
        methods: {
            debouncedFetch() {
                if (this.debounceTimer) clearTimeout(this.debounceTimer);
                this.debounceTimer = setTimeout(() => {
                    this.currentPage = 1;
                    this.fetchUsers();
                }, 300);
            },
            onRoleFilterChange() {
                // reset paging and refetch with the new role
                this.currentPage = 1;
                this.fetchUsers();
              },
            async fetchUsers() {
                this.loading = true;
                try {
                    const params = {
                        name: this.filters.name,
                        email: this.filters.email,
                        role: this.filters.role,
                        page: this.currentPage,
                        pageSize: this.pageSize
                    };
                    const res = await apiClient.get('/RoleManagement/users', { params });
                    const list = res.data?.data?.$values ?? res.data?.data ?? [];
                    this.users = list.map(x => ({
                        userId: x.userId,
                        firstName: x.firstName ?? '',
                        lastName: x.lastName ?? '',
                        email: x.email ?? '',
                        role: x.role ?? 'User',
                        isLocked: !!x.isLocked
                    }));
                    this.totalUsers = res.data?.total || this.users.length;
                } catch (e) {
                    console.error('Fetch users failed', e);
                    this.users = [];           // optional: clear on error
                    this.totalUsers = 0;
                } finally {
                    this.loading = false;
                }
            },

            // ===== Role change flow =====
            stashPrevRole(user) {
                user._stashedRole = user.role;
            },
            onRoleChange(user) {
                // what the role was before v-model changed it
                const prev = user._stashedRole ?? user.role;

                this.pendingUser = user;
                this.pendingPrevRole = prev;      // <-- use prev
                this.pendingNewRole = user.role;  // v-model has the new value now

                // keep UI showing the previous role until user confirms
                user.role = prev;

                this.showRoleConfirm = true;
            },
            cancelRoleChange() {
                this.showRoleConfirm = false;
                // ensure UI shows previous role
                if (this.pendingUser && this.pendingPrevRole != null) {
                    this.pendingUser.role = this.pendingPrevRole;
                }
                this.pendingUser = null;
                this.pendingPrevRole = null;
                this.pendingNewRole = null;
            },
            async confirmRoleChange() {
                const user = this.pendingUser;
                if (!user) return;
                this.saving.add('role-' + user.userId);
                try {
                    await apiClient.put(`/RoleManagement/${user.userId}/role`, { role: this.pendingNewRole });
                    user.role = this.pendingNewRole;  // commit to UI
                    user._stashedRole = this.pendingNewRole;
                    this.showRoleConfirm = false;
                    this.showRoleSuccess = true;
                } catch (e) {
                    console.error('Update role failed', e);
                    alert('Failed to update role.');
                    this.showRoleConfirm = false;
                } finally {
                    this.saving.delete('role-' + user.userId);
                }
            },

            // ===== Lock toggle flow =====
            onToggleLock(user, checked) {
                // revert UI immediately; only set after confirm
                this.pendingUser = user;
                this.willLock = checked;
                this.showLockConfirm = true;
                // revert visual toggle until confirmed
                user.isLocked = !checked;
            },
            cancelLockChange() {
                if (this.pendingUser) {
                    // ensure toggle goes back to original
                    this.pendingUser.isLocked = !this.willLock;
                }
                this.pendingUser = null;
                this.willLock = false;
                this.showLockConfirm = false;
            },
            async confirmLockChange() {
                const user = this.pendingUser;
                if (!user) return;
                this.saving.add('lock-' + user.userId);
                try {
                    await apiClient.put(`/RoleManagement/${user.userId}/lock`, { lock: this.willLock });
                    user.isLocked = this.willLock; // commit to UI
                    this.showLockConfirm = false;
                    this.showLockSuccess = true;
                } catch (e) {
                    console.error('Toggle lock failed', e);
                    alert('Failed to update lock status.');
                    // ensure UI reverts to prior state
                    user.isLocked = !this.willLock;
                    this.showLockConfirm = false;
                } finally {
                    this.saving.delete('lock-' + user.userId);
                }
            },

            changePage(p) {
                if (p < 1 || p > this.totalPages) return;
                this.currentPage = p;
                this.fetchUsers();
            }
        },
        watch: {
            'filters.role'() {
                this.currentPage = 1;
                this.fetchUsers();
            }
        }
    };</script>

<style scoped>
    .roles-container {
        padding: 24px 40px;
        font-family: 'Segoe UI', sans-serif;
        color: #333;
    }

    .header {
        display: flex;
        align-items: flex-end;
        justify-content: space-between;
        margin-bottom: 16px;
        gap: 12px;
    }

    .filter-panel {
        display: flex;
        gap: 12px;
        margin: 12px 0 16px;
        align-items: center;
    }

        .filter-panel input {
            flex: 1 1 0;
            min-width: 220px;
            padding: 10px;
            border-radius: 10px;
            border: 1px solid #ccd3de;
            font-size: 14px;
        }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        background: #fff;
        border-radius: 10px;
        box-shadow: 0 0 10px rgba(0,0,0,.05);
        overflow: hidden;
    }

        .modern-table th, .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e7ecf5;
            text-align: left;
        }

        .modern-table th {
            background: #f8fafc;
            font-weight: 600;
            color: #334155;
        }

        .modern-table tbody tr:hover {
            background: #f6f9ff;
        }

    .role-select {
        padding: 8px 10px;
        border-radius: 10px;
        border: 1px solid #d8dee8;
        font-size: 14px;
        min-width: 140px;
    }

    .badge {
        padding: 4px 10px;
        border-radius: 12px;
        font-size: 12px;
        font-weight: 700;
        border: 1px solid transparent;
    }

    .badge-user {
        background: #fff5f5;
        color: #b42318;
        border-color: #ffd3d3;
    }

    .badge-manager {
        background: #eef6ff;
        color: #1e60b2;
        border-color: #d7e8ff;
    }

    .badge-admin {
        background: #e9fbf1;
        color: #0b6b3a;
        border-color: #c6eed9;
    }

    .lock-pill {
        margin-left: 8px;
        font-size: 11px;
        background: #fee2e2;
        color: #b91c1c;
        border: 1px solid #fecaca;
        padding: 2px 8px;
        border-radius: 999px;
    }

    .pagination {
        margin-top: 18px;
        text-align: center;
    }

        .pagination button {
            background: #f1f5f9;
            border: 1px solid #cfd8e3;
            padding: 6px 12px;
            margin: 0 4px;
            border-radius: 6px;
            cursor: pointer;
        }

            .pagination button:disabled {
                opacity: .55;
                cursor: not-allowed;
            }

    .empty-message {
        text-align: center;
        color: #8a94a6;
        margin-top: 30px;
        font-style: italic;
    }

    /* simple iOS-style toggle */
    .switch {
        position: relative;
        display: inline-block;
        width: 46px;
        height: 24px;
    }

        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }

    .slider {
        position: absolute;
        cursor: pointer;
        inset: 0;
        background-color: #cfd8e3;
        transition: .2s;
        border-radius: 34px;
    }

        .slider:before {
            position: absolute;
            content: "";
            height: 18px;
            width: 18px;
            left: 3px;
            bottom: 3px;
            background-color: white;
            transition: .2s;
            border-radius: 50%;
        }

    input:checked + .slider {
        background-color: #22c55e;
    }

        input:checked + .slider:before {
            transform: translateX(22px);
        }

    /* Basic modal styles (reuse from your existing modals look) */
    .modal-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.6);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 2000;
    }

    .modal.confirmation, .modal.success {
        background: #fff;
        padding: 30px 20px;
        border-radius: 14px;
        width: 90%;
        max-width: 420px;
        box-shadow: 0 10px 30px rgba(0,0,0,.2);
        text-align: center;
    }

    .modal.confirmation {
        border-top: 6px solid #ffc107;
    }

    .modal.success {
        border-top: 6px solid #4caf50;
    }

    .close-btn {
        position: absolute;
        margin-left: calc(420px - 36px);
        margin-top: -8px;
        font-size: 22px;
        color: #999;
        background: none;
        border: none;
        cursor: pointer;
    }

    .button-group {
        display: flex;
        justify-content: center;
        gap: 20px;
        margin-top: 20px;
    }

    .btn-confirm {
        background-color: #4caf50;
        color: white;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-weight: bold;
        cursor: pointer;
    }

    .btn-cancel {
        background-color: #eee;
        color: #333;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-weight: bold;
        cursor: pointer;
    }

    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 8px 20px;
        border-radius: 6px;
        border: none;
        font-size: 14px;
        cursor: pointer;
    }
    .status-header {
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .role-filter {
        padding: 6px 8px;
        border: 1px solid #d8dee8;
        border-radius: 8px;
        font-size: 13px;
        background: #fff;
    }
    /* Loading skeleton */
    .skeleton-wrap {
        background: #fff;
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 0 10px rgba(0,0,0,.05);
    }

    .skeleton-header {
        display: flex;
        gap: 10px;
        margin-bottom: 14px;
    }

    .skeleton-pill {
        height: 14px;
        border-radius: 999px;
        background: linear-gradient(90deg, #eef2f7 25%, #f6f9ff 37%, #eef2f7 63%);
        background-size: 400% 100%;
        animation: shimmer 1.2s infinite;
    }

    .w-24 {
        width: 96px;
    }

    .w-28 {
        width: 112px;
    }

    .w-32 {
        width: 128px;
    }

    .w-40 {
        width: 160px;
    }

    .w-48 {
        width: 192px;
    }

    .w-56 {
        width: 224px;
    }

    .w-60 {
        width: 240px;
    }

    .skeleton-table {
        display: grid;
        gap: 10px;
    }

    .skeleton-row {
        display: grid;
        grid-template-columns: 1.2fr 1.4fr 1fr 0.6fr 0.8fr;
        gap: 14px;
        align-items: center;
    }

    .sk-cell {
        display: block;
        height: 16px;
        border-radius: 8px;
        background: linear-gradient(90deg, #eef2f7 25%, #f6f9ff 37%, #eef2f7 63%);
        background-size: 400% 100%;
        animation: shimmer 1.2s infinite;
    }

    .loading-text {
        margin-top: 14px;
        text-align: center;
        color: #64748b;
        font-size: 14px;
    }

    @keyframes shimmer {
        0% {
            background-position: 100% 0;
        }

        100% {
            background-position: 0 0;
        }
    }
</style>