<template>
    <div class="training-title-container">
        <div class="header">
            <h2>📘 Training Titles</h2>
        </div>

        <!-- Centered CTA -->
        <div class="cta-row">
            <button class="btn-primary btn-cta" @click="isModalOpen = true">
                ➕ Add New Title
            </button>
        </div>

        <!-- Filters: only live title search -->
        <div class="filter-panel">
            <div class="filter-group single">
                <input v-model="filters.title"
                       placeholder="Search titles…"
                       aria-label="Search titles" />
            </div>
        </div>

        <CreateTitleModal v-if="isModalOpen"
                          @close="isModalOpen = false"
                          @created="onTitleCreated" />

        <div class="table-wrapper" v-if="titles.length">
            <table class="modern-table">
                <thead>
                    <tr>
                        <th>Course Title</th>
                        <th style="width:180px">Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="title in titles" :key="title.subjectSysId">
                        <td class="truncate-cell" :title="title.courseTitle">
                            {{ title.courseTitle }}
                        </td>
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

        <EditTitleModal v-if="isEditModalOpen"
                        :title="editTitle"
                        @close="isEditModalOpen = false"
                        @updated="onTitleUpdated" />

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
                total: 0,
                currentPage: 1,
                pageSize: 10,
                filters: {
                    title: ""
                },
                // renamed to avoid vue/no-reserved-keys
                searchTimer: null
            };
        },
        computed: {
            totalPages() {
                return Math.ceil(this.total / this.pageSize);
            }
        },
        mounted() {
            this.fetchTitles();
        },
        beforeUnmount() {
            // clear any pending debounce on unmount
            clearTimeout(this.searchTimer);
        },
        watch: {
            // Live search with debounce (param removed to avoid no-unused-vars)
            "filters.title"() {
                clearTimeout(this.searchTimer);
                this.searchTimer = setTimeout(() => {
                    this.currentPage = 1;
                    this.fetchTitles();
                }, 300);
            }
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
            async fetchTitles() {
                const params = {
                    page: this.currentPage,
                    pageSize: this.pageSize
                };
                if (this.filters.title?.trim()) params.title = this.filters.title.trim();

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
        justify-content: center; /* center the heading */
        align-items: center;
        margin-bottom: 16px;
    }

        .header h2 {
            font-size: 28px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    /* Centered CTA row */
    .cta-row {
        display: flex;
        justify-content: center;
        margin-bottom: 18px;
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

    /* Bigger CTA */
    .btn-cta {
        padding: 14px 28px;
        font-size: 18px;
        border-radius: 12px;
    }

    /* Filters: only the search box now */
    .filter-panel {
        background: #f9fafb;
        padding: 16px 20px;
        border-radius: 16px;
        margin-bottom: 24px;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
    }

    .filter-group.single {
        display: grid;
        grid-template-columns: minmax(240px, 480px);
        justify-content: center;
        gap: 12px;
        margin: 0 auto;
    }

    .filter-group input {
        padding: 10px 14px;
        border: 1px solid #ccc;
        border-radius: 12px;
        font-size: 14px;
        background: #fff;
        transition: border 0.3s ease;
    }

        .filter-group input:focus {
            border-color: #4caf50;
            outline: none;
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
        max-width: 520px; /* a bit wider since category column is gone */
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
</style>