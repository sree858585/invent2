<template>
    <div class="training-title-container">
        <div class="header">
            <h2>📘 Training Titles</h2>
            <button class="btn-primary" @click="isModalOpen = true">➕ Create New Title</button>
        </div>

        <div class="filter-panel">
            <div class="filter-group">
                <input v-model="filters.title" placeholder="Search Title..." />
                <select v-model="filters.category">
                    <option value="">All Categories</option>
                    <option v-for="c in categories" :key="c.code" :value="c.code">{{ c.value }}</option>
                </select>
                <button class="btn-search" @click="applyFilters">Search</button>
                <button class="btn-secondary" @click="resetFilters">Reset</button>
            </div>
        </div>

        <CreateTitleModal v-if="isModalOpen" @close="isModalOpen = false" @created="onTitleCreated" />

        <div class="table-wrapper" v-if="titles.length">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Course Title</th>
                        <th>Category</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="title in titles" :key="title.subjectSysId">
                        <td class="truncate-cell" :title="title.courseTitle">
                            {{ title.courseTitle }}
                        </td>
                        <td>{{ title.categoryName }}</td>
                        <td>
                            <button class="btn-action" @click="openEditModal(title)">✏️ Edit</button>
                            <button class="btn-danger" @click="openDeleteModal(title)">🗑 Delete</button>
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination" v-if="totalPages > 1">
                <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">⏮ Prev</button>
                <span>Page {{ currentPage }} of {{ totalPages }}</span>
                <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">Next ⏭</button>
            </div>
        </div>

        <p v-else class="no-data">No training titles found.</p>
        <EditTitleModal v-if="isEditModalOpen" :title="editTitle" @close="isEditModalOpen = false" @updated="onTitleUpdated" />
        <DeleteConfirmModal v-if="deleteModalOpen"
                            :title="deleteTitle"
                            @close="deleteModalOpen = false"
                            @deleted="handleDelete" />
    </div>
</template>

<script>import CreateTitleModal from "@/components/Modals/CreateTitleModal.vue";
    import EditTitleModal from "@/components/Modals/EditTitleModal.vue";
    import DeleteConfirmModal from "@/components/Modals/DeleteConfirmModal.vue";
    import apiClient from "@/axios";

    export default {
        components: { CreateTitleModal, EditTitleModal, DeleteConfirmModal },
        data() {
            return {
                isModalOpen: false,
                isEditModalOpen: false,
                editTitle: null,
                deleteModalOpen: false,
                deleteTitle: null,
                titles: [],
                categories: [],
                total: 0,
                currentPage: 1,
                pageSize: 10,
                filters: {
                    title: "",
                    category: ""
                }
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.total / this.pageSize);
            }
        },
        mounted() {
            this.loadCategories();
            this.fetchTitles();
        },
        methods: {
            openDeleteModal(title) {
                this.deleteTitle = title;
                this.deleteModalOpen = true;
            },
            openEditModal(title) {
                this.editTitle = title;
                this.isEditModalOpen = true;
            },
            onTitleUpdated() {
                this.isEditModalOpen = false;
                this.fetchTitles();
            },
            async loadCategories() {
                const res = await apiClient.get("/Lookup/categories");
                this.categories = res.data?.$values ?? [];
            },
            async fetchTitles() {
                const params = {
                    page: this.currentPage,
                    pageSize: this.pageSize
                };

                if (this.filters.title?.trim()) params.title = this.filters.title.trim();
                if (this.filters.category) params.category = this.filters.category;

                const res = await apiClient.get("/TrainingTitle/paged", { params });
                this.titles = res.data?.data?.$values || [];
                this.total = res.data?.total || 0;
            },
            async handleDelete(id) {
                try {
                    await apiClient.delete(`/TrainingTitle/delete/${id}`);
                    this.deleteModalOpen = false;
                    this.fetchTitles();
                    alert("Training title deleted successfully!");
                } catch (err) {
                    console.error("❌ Failed to delete:", err);
                    alert("Error deleting title.");
                }
            },
            applyFilters() {
                this.currentPage = 1;
                this.fetchTitles();
            },
            resetFilters() {
                this.filters.title = "";
                this.filters.category = "";
                this.currentPage = 1;
                this.fetchTitles();
            },
            onTitleCreated() {
                this.isModalOpen = false;
                this.currentPage = 1;
                this.fetchTitles();
            },
            changePage(page) {
                if (page < 1 || page > this.totalPages) return;
                this.currentPage = page;
                this.fetchTitles();
            }
        }
    };</script>

<style scoped>
    .training-title-container {
        padding: 32px;
        font-family: "Segoe UI", sans-serif;
        color: #333;
    }

    .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 24px;
    }

        .header h2 {
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .btn-primary {
        background-color: #4caf50;
        color: white;
        border: none;
        padding: 10px 20px;
        font-size: 16px;
        border-radius: 10px;
        cursor: pointer;
        transition: background-color 0.3s ease;
    }

        .btn-primary:hover {
            background-color: #388e3c;
        }

    .filter-panel {
        background: #f9fafb;
        padding: 20px;
        border-radius: 16px;
        margin-bottom: 24px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
    }

    .filter-group {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 16px;
    }

        .filter-group input,
        .filter-group select {
            padding: 10px 14px;
            border: 1px solid #ccc;
            border-radius: 12px;
            font-size: 14px;
            background: #fff;
            transition: border 0.3s ease;
        }

            .filter-group input:focus,
            .filter-group select:focus {
                border-color: #4caf50;
                outline: none;
            }

    .btn-search {
        background-color: #007bff;
        color: white;
        padding: 10px 18px;
        font-size: 14px;
        border: none;
        border-radius: 8px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-search:hover {
            background-color: #0056b3;
        }

    .table-wrapper {
        overflow-x: auto;
        margin-top: 20px;
    }

    .modern-table {
        width: 100%;
        border-collapse: collapse;
        background-color: #fff;
        border-radius: 10px;
        overflow: hidden;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
    }

        .modern-table th,
        .modern-table td {
            padding: 14px;
            border-bottom: 1px solid #e0e0e0;
            text-align: left;
        }

        .modern-table th {
            background-color: #f8f9fa;
            font-weight: 600;
        }

    .btn-action {
        background-color: #007bff;
        color: white;
        border: none;
        padding: 6px 12px;
        border-radius: 6px;
        margin-right: 8px;
        cursor: pointer;
    }

    .btn-danger {
        background-color: #e74c3c;
        color: white;
        border: none;
        padding: 6px 12px;
        border-radius: 6px;
        cursor: pointer;
    }

    .no-data {
        text-align: center;
        margin-top: 40px;
        color: #777;
        font-size: 16px;
    }

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
                cursor: not-allowed;
                opacity: 0.5;
            }
    .truncate-cell {
        max-width: 300px; /* or whatever fits your layout */
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
</style>