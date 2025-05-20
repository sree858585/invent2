<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Add User to Course</h3>
            <p><strong>Course:</strong> {{ course?.subjectTitle }}</p>

            <div class="search-fields">
                <input v-model="lastName" placeholder="Search by Last Name" @input="fetchUsers" />
                <input v-model="email" placeholder="Search by Email" @input="fetchUsers" />
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
                        <td>
                            <span v-if="isRegistered(user.userSysId)">✅ Registered</span>
                            <button v-else class="btn-green" @click="addUser(user)">
                                {{ isDropped(user.userSysId) ? 'Re-register' : 'Add' }}
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>

            <p v-if="!users.length && (lastName || email)">No users found.</p>
            <button class="btn-danger" @click="$emit('close')">Close</button>
        </div>
    </div>
    <!-- Confirmation Modal -->
    <div v-if="showConfirmDialog" class="modal-overlay">
        <div class="modal confirmation">
            <h3>Confirm User Addition</h3>
            <p>Are you sure you want to add <strong>{{ selectedUser?.fullName }}</strong> to this course?</p>
            <div class="button-group">
                <button class="btn-green" @click="confirmAddUser">Add User</button>
                <button class="btn-secondary" @click="showConfirmDialog = false">Cancel</button>
            </div>
        </div>
    </div>

    <!-- Success Modal -->
    <div v-if="showSuccessDialog" class="modal-overlay">
        <div class="modal success">
            <h3>User Added</h3>
            <p><strong>{{ selectedUser?.fullName }}</strong> has been successfully added to this course.</p>
            <div class="button-group">
                <button class="btn-primary" @click="closeSuccessDialog">Back</button>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";

export default {
  props: ['course'],
  data() {
    return {
      lastName: "",
      email: "",
      users: [],
      selectedUser: null,
      showConfirmDialog: false,
      showSuccessDialog: false,
      registeredUserIds: [],
      registeredUserStatus: []
    };
  },
  mounted() {
    this.fetchRegisteredUserIds(); // Initial load
  },
  methods: {
    async fetchRegisteredUserIds() {
  try {
    const res = await apiClient.get(`/CourseAdmin/registered-user-status?courseId=${this.course.courseSysId}`);
    const raw = res.data?.$values ?? res.data ?? [];

    // Store both user ID and status
    this.registeredUserStatus = raw.map(r => ({
      userSysId: parseInt(r.userSysId),
      status: parseInt(r.status)
    }));
  } catch (err) {
    console.error("❌ Failed to load registered user statuses", err);
    this.registeredUserStatus = [];
  }
},

    async fetchUsers() {
      const params = {};
      if (this.lastName) params.lastName = this.lastName;
      if (this.email) params.email = this.email;

      try {
        const res = await apiClient.get("/CourseAdmin/search-users", { params });
        this.users = Array.isArray(res.data) ? res.data : res.data?.$values ?? [];
        console.log("🔍 Users loaded:", this.users);

        // Always refresh registered users after search
        await this.fetchRegisteredUserIds();
      } catch (err) {
        console.error("Error fetching users or registered list:", err);
        this.users = [];
        this.registeredUserIds = [];
      }
    },

    isRegistered(userId) {
  const entry = this.registeredUserStatus.find(u => u.userSysId === parseInt(userId));
  return entry?.status === 1;
},
isDropped(userId) {
  const entry = this.registeredUserStatus.find(u => u.userSysId === parseInt(userId));
  return entry?.status === 6;
},

    async addUser(user) {
      this.selectedUser = user;
      this.showConfirmDialog = true;
    },

    async confirmAddUser() {
      try {
        const payload = {
          userSysId: this.selectedUser.userSysId,
          courseSysId: this.course.courseSysId,
          dateEntered: new Date().toISOString(),
          dateModified: new Date().toISOString(),
          status: 1,
          dateStatusChanged: new Date().toISOString(),
          hybrid: 0,
          isWaitlisted: false
        };

        await apiClient.post('/CourseAdmin/add-user-to-course', payload);
        this.showConfirmDialog = false;
        this.showSuccessDialog = true;

        this.$emit('user-changed', { courseSysId: this.course.courseSysId, delta: 1 });

        // ✅ Refresh registered list so UI reflects "Registered"
        await this.fetchRegisteredUserIds();
      } catch (err) {
        console.error("❌ Failed to add user to course", err);
        alert("Failed to add user. Please try again.");
        this.showConfirmDialog = false;
      }
    },

    async closeSuccessDialog() {
      this.showSuccessDialog = false;
      this.selectedUser = null;
      await this.fetchUsers(); // Reload both user list & registered list
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
</style>