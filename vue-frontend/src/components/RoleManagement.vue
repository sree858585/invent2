<template>
    <div class="roles-container">
        <div class="header">
            <h2>🛡️ Role Management</h2>
        </div>

        <div class="filter-panel">
            <input v-model="filters.name"
                   placeholder="🔍 Search by Full Name"
                   @input="debouncedFetch()" />
            <input v-model="filters.email"
                   placeholder="✉️ Search by Email"
                   @input="debouncedFetch()" />
        </div>

        <table class="modern-table" v-if="users.length">
            <thead>
                <tr>
                    <th class="sortable" @click="toggleSort('name')">
                        Full Name
                        <span class="sort-indicator" v-if="sort.by==='name'">{{ sort.dir==='asc' ? '▲' : '▼' }}</span>
                    </th>
                    <th>Email</th>
                    <th>Role</th>

                    <!-- Actions header with explicit sort button -->
                    <th style="width:160px;">
                        <div class="th-flex">
                            Actions
                            <button class="sort-btn"
                                    @click.stop="toggleSort('role')"
                                    :aria-label="`Sort by role ${sort.dir==='asc' ? 'ascending' : 'descending'}`">
                                <span v-if="sort.by==='role'">{{ sort.dir==='asc' ? '▲' : '▼' }}</span>
                                <span v-else>⇅</span>
                            </button>
                        </div>
                    </th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="u in users" :key="u.userId">
                    <td>{{ u.firstName }} {{ u.lastName }}</td>
                    <td>{{ u.email }}</td>
                    <td>
                        <select class="role-select" v-model="u.role" @change="saveRole(u)">
                            <option value="User">User</option>
                            <option value="Manager">Manager</option>
                            <option value="Admin">Admin</option>
                        </select>
                    </td>
                    <td>
                        <span class="badge" :class="'badge-' + u.role.toLowerCase()">{{ u.role }}</span>
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
    </div>
</template>

<script>import apiClient from '@/axios.js';

    export default {
        name: 'RoleManagement',
        data() {
            return {
                users: [],
                totalUsers: 0,
                currentPage: 1,
                pageSize: 10,
                filters: { name: '', email: '' },
                sort: { by: 'name', dir: 'asc' }, // 'name' | 'role'
                saving: new Set(),
                debounceTimer: null,              
            };
        },
        computed: {
            totalPages() { return Math.ceil(this.totalUsers / this.pageSize); }
        },
        mounted() { this.fetchUsers(); },
        beforeUnmount() {                    
            if (this.debounceTimer) clearTimeout(this.debounceTimer);
        },
        methods: {
            debouncedFetch() {
                if (this.debounceTimer) clearTimeout(this.debounceTimer); 
                this.debounceTimer = setTimeout(() => this.fetchUsers(), 300);
            },
            toggleSort(by) {
                if (this.sort.by === by) {
                    this.sort.dir = this.sort.dir === 'asc' ? 'desc' : 'asc';
                } else {
                    this.sort.by = by;
                    this.sort.dir = 'asc';
                }
                this.fetchUsers();
            },
            async fetchUsers() {
                const params = {
                    name: this.filters.name,
                    email: this.filters.email,
                    page: this.currentPage,
                    pageSize: this.pageSize,
                    sortBy: this.sort.by,
                    sortDir: this.sort.dir
                };
                try {
                    const res = await apiClient.get('/RoleManagement/users', { params });
                    const list = res.data?.data?.$values ?? res.data?.data ?? [];
                    this.users = list.map(x => ({
                        userId: x.userId,
                        firstName: x.firstName ?? '',
                        lastName: x.lastName ?? '',
                        email: x.email ?? '',
                        role: x.role ?? 'User'
                    }));
                    this.totalUsers = res.data?.total || this.users.length;
                } catch (e) {
                    console.error('Fetch users failed', e);
                }
            },
            async saveRole(user) {
                if (this.saving.has(user.userId)) return;
                this.saving.add(user.userId);
                const previous = user.role;
                try {
                    await apiClient.put(`/RoleManagement/${user.userId}/role`, { role: user.role });
                } catch (e) {
                    console.error('Update role failed', e);
                    user.role = previous;
                    alert('Failed to update role.');
                } finally {
                    this.saving.delete(user.userId);
                }
            },
            changePage(p) {
                if (p < 1 || p > this.totalPages) return;
                this.currentPage = p;
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
            min-width: 220px; /* keeps them usable on smaller screens */
            padding: 10px;
            border-radius: 10px;
            border: 1px solid #ccd3de;
            font-size: 14px;
        }
    .th-flex {
        display: inline-flex;
        align-items: center;
        gap: 8px;
    }

    .sort-btn {
        border: 1px solid #d9e0ea;
        background: #f8fafc;
        border-radius: 8px;
        padding: 4px 8px;
        font-size: 12px;
        line-height: 1;
        cursor: pointer;
        color: #334155;
    }

        .sort-btn:hover {
            background: #eef2f7;
        }

    /* Keep pointer on other sortable headers */
    .modern-table th.sortable {
        cursor: pointer;
        user-select: none;
    }

    .sort-indicator {
        margin-left: 6px;
        font-size: 12px;
        color: #64748b;
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
    .modern-table th.sortable {
        cursor: pointer;
        user-select: none;
    }

    .sort-indicator {
        margin-left: 6px;
        font-size: 12px;
        color: #64748b;
    }
</style>