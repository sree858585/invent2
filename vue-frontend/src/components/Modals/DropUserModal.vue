<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Drop User from Course</h3>
            <p><strong>Course:</strong> {{ course?.subjectTitle }}</p>

            <div class="search-fields">
                <input v-model="lastName" placeholder="Search by Last Name" @input="fetchRegisteredUsers" />
                <input v-model="email" placeholder="Search by Email" @input="fetchRegisteredUsers" />
            </div>

            <table class="user-table" v-if="users.length > 0">
                <thead>
                    <tr>
                        <th>Full Name</th>
                        <th>Email</th>
                        <th>Role</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="user in users" :key="user.userSysId">
                        <td>{{ user.fullName }}</td>
                        <td>{{ user.email }}</td>
                        <td>{{ user.role }}</td>
                        <td><button class="btn-danger" @click="promptDropUser(user)">Drop</button></td>
                    </tr>
                </tbody>
            </table>
            <div class="pagination modern-pagination" v-if="totalPages > 1">
                <button @click="currentPage--" :disabled="currentPage === 1">⏮ Prev</button>
                <span>Page {{ currentPage }} of {{ totalPages }}</span>
                <button @click="currentPage++" :disabled="currentPage === totalPages">Next ⏭</button>
            </div>

            <p v-if="!users.length && (lastName || email)">No matching users found.</p>
            <button class="btn-danger" @click="$emit('close')">Close</button>
        </div>
    </div>

    <!-- Confirmation Modal -->
    <div v-if="showConfirmDialog" class="modal-overlay">
        <div class="modal confirmation">
            <h3>Confirm Drop</h3>
            <p>Are you sure you want to drop <strong>{{ selectedUser?.fullName }}</strong> from this course?</p>
            <div class="button-group">
                <button class="btn-danger" @click="confirmDropUser">Yes, Drop</button>
                <button class="btn-secondary" @click="showConfirmDialog = false">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Success Modal -->
    <div v-if="showSuccessDialog" class="modal-overlay">
        <div class="modal success">
            <h3>User Dropped</h3>
            <p><strong>{{ selectedUser?.fullName }}</strong> has been successfully dropped from the course.</p>
            <div class="button-group">
                <button class="btn-primary" @click="closeSuccessDialog">Back</button>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios.js';

export default {
  props: ['course'],
  data() {
  return {
    lastName: '',
    email: '',
    users: [],
    currentPage: 1,
    totalPages: 1,
    selectedUser: null,
    showConfirmDialog: false,
    showSuccessDialog: false
  };
},
  mounted() {
    this.fetchRegisteredUsers();
  },
  watch: {
    currentPage() {
      this.fetchRegisteredUsers();
    }
  },

  methods: {
    async fetchRegisteredUsers() {
  const params = {
    courseId: this.course.courseSysId,
    lastName: this.lastName,
    email: this.email,
    page: this.currentPage,
    pageSize: 15
  };

  try {
    const res = await apiClient.get('/CourseAdmin/registered-users', { params });
    const data = res.data;
this.users = data.data?.$values || [];
this.totalPages = Math.ceil(data.total / 15);
  } catch (err) {
    console.error('Error fetching registered users:', err);
    this.users = [];
  }
},

    promptDropUser(user) {
      this.selectedUser = user;
      this.showConfirmDialog = true;
    },

    async confirmDropUser() {
      try {
        const payload = {
          courseSysId: this.course.courseSysId,
          userSysId: this.selectedUser.userSysId
        };

        await apiClient.put('/CourseAdmin/drop-user', payload);
        this.showConfirmDialog = false;
        this.showSuccessDialog = true;
        this.$emit('user-changed', { courseSysId: this.course.courseSysId, delta: -1 });
        await this.fetchRegisteredUsers();
      } catch (err) {
        console.error('❌ Failed to drop user:', err);
        alert('Failed to drop user.');
        this.showConfirmDialog = false;
      }
    },

    closeSuccessDialog() {
      this.showSuccessDialog = false;
      this.selectedUser = null;
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

        .modal h3 {
            font-size: 24px;
            font-weight: 600;
            margin-bottom: 16px;
            text-align: center;
            color: #1f1f1f;
        }

    .search-fields {
        display: flex;
        gap: 12px;
        margin-bottom: 24px;
    }

        .search-fields input {
            flex: 1;
            padding: 12px;
            border: 1px solid #ccc;
            border-radius: 10px;
            font-size: 15px;
            background-color: #fafafa;
            transition: all 0.2s ease;
        }

            .search-fields input:focus {
                border-color: #3f51b5;
                background-color: #fff;
                outline: none;
                box-shadow: 0 0 0 2px rgba(63, 81, 181, 0.15);
            }

    .user-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 24px;
    }

        .user-table th,
        .user-table td {
            padding: 12px 16px;
            text-align: left;
            border-bottom: 1px solid #e0e0e0;
            font-size: 15px;
        }

        .user-table th {
            background-color: #f5f5f5;
            font-weight: 600;
            color: #333;
        }

        .user-table tr:hover {
            background-color: #f9f9f9;
        }

    .btn-green {
        background-color: #4CAF50;
        color: white;
        padding: 8px 16px;
        border: none;
        border-radius: 8px;
        font-size: 14px;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

        .btn-green:hover {
            background-color: #43a047;
        }

    .btn-danger {
        background-color: #f44336;
        color: white;
        padding: 10px 20px;
        border: none;
        font-size: 14px;
        border-radius: 8px;
        cursor: pointer;
        margin-top: 12px;
    }

        .btn-danger:hover {
            background-color: #d32f2f;
        }

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

    @media (max-width: 768px) {
        .modal {
            width: 95%;
            padding: 24px;
        }

        .search-fields {
            flex-direction: column;
        }
    }
    /* === Confirmation Modal === */
    .modal.confirmation {
        border-top: 6px solid #ffc107;
        animation: fadeInScale 0.3s ease;
    }

        .modal.confirmation h3 {
            color: #ff9800;
            font-weight: bold;
            margin-bottom: 10px;
            text-align: center;
        }

        .modal.confirmation p {
            font-size: 16px;
            color: #444;
            margin: 16px 0;
            text-align: center;
        }

    .modal .button-group {
        display: flex;
        justify-content: center;
        gap: 20px;
        margin-top: 20px;
    }

    .btn-secondary {
        background-color: #e0e0e0;
        color: #333;
        padding: 8px 16px;
        border-radius: 8px;
        border: none;
        font-size: 14px;
        cursor: pointer;
    }

        .btn-secondary:hover {
            background-color: #cfcfcf;
        }

    /* === Success Modal === */
    .modal.success {
        border-top: 6px solid #4CAF50;
        animation: fadeInScale 0.3s ease;
    }

        .modal.success h3 {
            color: #388e3c;
            font-weight: bold;
            margin-bottom: 10px;
            text-align: center;
        }

        .modal.success p {
            font-size: 16px;
            color: #444;
            margin: 16px 0;
            text-align: center;
        }

    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 8px 20px;
        border-radius: 8px;
        border: none;
        font-size: 14px;
        cursor: pointer;
    }

        .btn-primary:hover {
            background-color: #1565c0;
        }

    /* Slight animation for pop-up */
    @keyframes fadeInScale {
        from {
            opacity: 0;
            transform: scale(0.9);
        }

        to {
            opacity: 1;
            transform: scale(1);
        }
    }
    .modern-pagination {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 16px;
        margin-top: 24px;
        font-size: 15px;
        color: #333;
    }

        .modern-pagination button {
            background: #f0f0f0;
            color: #333;
            border: 1px solid #ccc;
            padding: 8px 14px;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.2s ease;
            font-weight: 500;
        }

            .modern-pagination button:hover:not(:disabled) {
                background-color: #e0e0e0;
            }

            .modern-pagination button:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }
</style>
