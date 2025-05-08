<template>
    <div class="modal-overlay">
        <div class="modal">
            <h3>Email Registered Users</h3>
            <p><strong>Course:</strong> {{ course?.subjectTitle }}</p>

            <div class="email-form">
                <!-- Select All Checkbox -->
                <div class="select-all">
                    <label>
                        <input type="checkbox" v-model="selectAll" @change="toggleSelectAll" />
                        Select All Users
                    </label>
                </div>

                <!-- Registered Users Table -->
                <table class="user-table">
                    <thead>
                        <tr>
                            <th>Select</th>
                            <th>Full Name</th>
                            <th>Email</th>
                            <th>Role</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="user in users" :key="user.userSysId">
                            <td>
                                <input type="checkbox"
                                       :value="user.userSysId"
                                       v-model="selectedUserIds" />
                            </td>
                            <td>{{ user.fullName }}</td>
                            <td>{{ user.email }}</td>
                            <td>{{ user.role }}</td>
                        </tr>
                    </tbody>
                </table>

                <div class="email-preview">
                    <strong>Selected Emails:</strong>
                    {{ displayedEmails.join(', ') }}
                    <span v-if="hasMoreEmails"
                          class="toggle-link"
                          @click="showAllEmails = !showAllEmails"
                          :title="showAllEmails ? 'Show less' : 'Show all'">
                        <span v-if="showAllEmails">▲</span>
                        <span v-else>▼</span>
                    </span>
                </div>

                <!-- Email Form Fields -->
                <div class="email-fields">
                    <label>CC:</label>
                    <input type="email" v-model="ccEmail" class="cc-input" />

                    <label>Subject:</label>
                    <input type="text" v-model="emailSubject" placeholder="Enter email subject..." />

                    <label>Message:</label>
                    <quill-editor v-model:content="emailBody"
                                  contentType="html"
                                  theme="snow"
                                  class="quill-box"
                                  placeholder="Enter email content here..." />
                </div>

                <!-- Actions -->
                <div class="button-group">
                    <button class="btn-primary" @click="sendEmail">Send Email</button>
                    <button class="btn-secondary" @click="$emit('close')">Cancel</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script>import apiClient from '@/axios';
    import { QuillEditor } from '@vueup/vue-quill';

    export default {
        components: { QuillEditor },
        props: ['course', 'currentUser'],

        data() {
            return {
                ccEmail: '',
                showAllEmails: false,
                EMAIL_DISPLAY_LIMIT: 10,
                users: [],
                selectedUserIds: [],
                selectAll: false,
                emailSubject: '',
                emailBody: '',
                registeredUsers: []
            };
        },

        computed: {
            selectedEmails() {
                return this.users
                    .filter(user => this.selectedUserIds.includes(user.userSysId))
                    .map(user => user.email);
            },
            displayedEmails() {
                return this.showAllEmails
                    ? this.selectedEmails
                    : this.selectedEmails.slice(0, this.EMAIL_DISPLAY_LIMIT);
            },
            hasMoreEmails() {
                return this.selectedEmails.length > this.EMAIL_DISPLAY_LIMIT;
            }
        },

        async mounted() {
            const userId = localStorage.getItem("userId");
            if (userId) {
                try {
                    const res = await apiClient.get(`/User/${userId}`);
                    this.ccEmail = res.data?.email || '';
                } catch (err) {
                    console.warn("⚠️ Failed to load user for CC", err);
                }
            }

            await this.fetchRegisteredUsers();
        },

        methods: {
            async fetchRegisteredUsers() {
                try {
                    const res = await apiClient.get('/CourseAdmin/registered-users', {
                        params: { courseId: this.course.courseSysId, pageSize: 1000 }
                    });

                    this.users = res.data?.data?.$values ?? [];
                    console.log("✅ Registered Users Loaded:", this.users);
                } catch (err) {
                    console.error('❌ Failed to load registered users', err);
                    this.users = [];
                }
            },

            toggleSelectAll() {
                this.selectedUserIds = this.selectAll ? this.users.map(u => u.userSysId) : [];
            },

            async sendEmail() {
                if (!this.emailSubject.trim() || !this.emailBody.trim()) {
                    alert('Please enter both subject and message.');
                    return;
                }

                if (this.selectedUserIds.length === 0) {
                    alert('Please select at least one user to send the email.');
                    return;
                }

                try {
                    await apiClient.post('/Email/send', {
                        userIds: this.selectedUserIds,
                        courseId: this.course.courseSysId,
                        subject: this.emailSubject,
                        message: this.emailBody,
                        cc: this.ccEmail
                    });

                    alert('✅ Email sent successfully!');
                    this.$emit('close');
                } catch (err) {
                    console.error('❌ Failed to send email', err);
                    alert('Failed to send email. Please try again.');
                }
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
        z-index: 9999;
    }

    .modal {
        background: white;
        padding: 30px;
        border-radius: 14px;
        width: 980px;
        max-height: 92vh;
        overflow-y: auto;
        font-family: 'Segoe UI', sans-serif;
    }

    h3 {
        margin-bottom: 10px;
        text-align: center;
        color: #222;
    }

    .select-all {
        margin-bottom: 10px;
        font-weight: 600;
    }

    .user-table {
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 20px;
    }

    .user-table th,
    .user-table td {
        padding: 10px;
        text-align: left;
        border-bottom: 1px solid #eee;
    }

    .user-table th {
        background-color: #f5f5f5;
        font-weight: bold;
    }
    .email-fields {
        margin-bottom: 20px;
    }

    .email-fields label {
        font-weight: 600;
        display: block;
        margin-top: 12px;
    }

    .email-fields input {
        width: 100%;
        padding: 12px;
        margin-top: 4px;
        border: 1px solid #ccc;
        border-radius: 8px;
        font-size: 15px;
    }

    .quill-box {
        margin-top: 10px;
        border-radius: 10px;
        border: 1px solid #ccc;
        background-color: #fff;
        height: auto;
        min-height: 500px; /* Increased minimum height */
        max-height: 700px; /* More space for typing */
        overflow: hidden; /* prevent scrollbar duplication */
    }

        .quill-box .ql-editor {
            font-family: 'Segoe UI', sans-serif;
            font-size: 16px;
            color: #333;
            padding: 20px;
            line-height: 1.6;
            min-height: 480px; /* Match larger size */
            max-height: 660px;
            overflow-y: auto;
            box-sizing: border-box;
            border-radius: 0 0 10px 10px;
        }

    .button-group {
        display: flex;
        justify-content: flex-end;
        margin-top: 20px;
        gap: 12px;
    }

    .btn-primary {
        background-color: #1976d2;
        color: white;
        padding: 10px 20px;
        font-weight: 600;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }

    .btn-secondary {
        background-color: #eee;
        color: #333;
        padding: 10px 20px;
        border-radius: 8px;
        border: none;
        cursor: pointer;
    }

    .user-table {
        max-height: 300px;
        overflow-y: auto;
        display: block;
    }

    .email-preview {
        margin-top: 8px;
        font-size: 14.5px;
        color: #333;
        display: flex;
        align-items: center;
        gap: 8px;
        flex-wrap: wrap;
    }

    .toggle-link {
        color: #1976d2;
        cursor: pointer;
        font-weight: bold;
        padding-left: 4px;
        font-size: 16px;
        user-select: none;
        transition: transform 0.2s;
    }

    .toggle-link:hover {
        text-decoration: underline;
    }

    .cc-input {
        width: 100%;
        padding: 12px;
        font-size: 15px;
        border-radius: 8px;
        border: 1px solid #ccc;
        margin-bottom: 10px;
    }
</style>
