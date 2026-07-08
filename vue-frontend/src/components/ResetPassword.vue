<template>
    <div class="reset-page">
        <div class="reset-card">
            <h2>Reset Password</h2>
            <p>Please enter your new password below.</p>

            <div class="password-rules">
                <strong>Password must contain:</strong>
                <ul>
                    <li :class="{ valid: hasMinLength }">At least 8 characters</li>
                    <li :class="{ valid: hasUppercase }">One uppercase letter</li>
                    <li :class="{ valid: hasNumber }">One number</li>
                    <li :class="{ valid: hasSpecialChar }">One special character</li>
                </ul>
            </div>

            <input type="password" v-model="newPassword" placeholder="New password" />
            <input type="password" v-model="confirmPassword" placeholder="Confirm password" />

            <button v-if="!message" @click="resetPassword" :disabled="loading">
                <span v-if="loading" class="spinner"></span>
                {{ loading ? "Resetting..." : "Reset Password" }}
            </button>

            <p v-if="message" class="message">{{ message }}</p>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";

export default {
    data() {
        return {
            email: "",
            token: "",
            newPassword: "",
            confirmPassword: "",
            loading: false,
            message: ""
        };
    },

    computed: {
        hasMinLength() {
            return this.newPassword.length >= 8;
        },
        hasUppercase() {
            return /[A-Z]/.test(this.newPassword);
        },
        hasNumber() {
            return /\d/.test(this.newPassword);
        },
        hasSpecialChar() {
            return /[^A-Za-z0-9]/.test(this.newPassword);
        },
        isPasswordValid() {
            return this.hasMinLength &&
                this.hasUppercase &&
                this.hasNumber &&
                this.hasSpecialChar;
        }
    },

    mounted() {
        this.email = this.$route.query.email || "";
        this.token = this.$route.query.token || "";
    },

    methods: {
        async resetPassword() {
            if (!this.newPassword || !this.confirmPassword) {
                alert("Please enter and confirm your password.");
                return;
            }

            if (!this.isPasswordValid) {
                alert("Password must have at least 8 characters, one uppercase letter, one number, and one special character.");
                return;
            }

            if (this.newPassword !== this.confirmPassword) {
                alert("Passwords do not match.");
                return;
            }

            this.loading = true;
            this.message = "";

            try {
                const response = await apiClient.post("/login/reset-password", {
                    email: this.email,
                    token: this.token,
                    newPassword: this.newPassword
                });

                this.message = response.data.message || "Password reset successfully. Redirecting to login...";

                this.newPassword = "";
                this.confirmPassword = "";

                setTimeout(() => {
                    this.$router.replace({
                        path: "/home",
                        query: { login: "true" }
                    });
                }, 1500);
            } catch (error) {
                alert(error.response?.data?.message || "Password reset failed.");
            } finally {
                this.loading = false;
            }
        }
    }
};</script>

<style scoped>
    .reset-page {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #f4eff9;
    }

    .reset-card {
        width: 420px;
        background: white;
        padding: 32px;
        border-radius: 22px;
        box-shadow: 0 20px 50px rgba(15, 23, 42, 0.18);
    }

        .reset-card h2 {
            color: #43285D;
            margin-bottom: 8px;
        }

        .reset-card p {
            color: #5f5968;
        }

        .reset-card input {
            width: 100%;
            padding: 14px;
            margin-top: 14px;
            border-radius: 12px;
            border: 1px solid #d8d2df;
            box-sizing: border-box;
        }

        .reset-card button {
            width: 100%;
            margin-top: 18px;
            padding: 14px;
            border: none;
            border-radius: 12px;
            background: #43285D;
            color: white;
            font-weight: 800;
            cursor: pointer;
        }

            .reset-card button:disabled {
                opacity: 0.75;
                cursor: not-allowed;
            }

    .message {
        margin-top: 16px;
        padding: 12px;
        background: #ecfdf3;
        color: #166534;
        border-radius: 10px;
        font-weight: 700;
    }

    .spinner {
        display: inline-block;
        width: 15px;
        height: 15px;
        margin-right: 8px;
        border: 2px solid rgba(255,255,255,0.45);
        border-top-color: white;
        border-radius: 50%;
        animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
        to {
            transform: rotate(360deg);
        }
    }
    .password-rules {
        background: #f4eff9;
        border: 1px solid #d8c8e6;
        border-radius: 12px;
        padding: 12px 14px;
        margin-top: 14px;
        font-size: 14px;
        color: #43285D;
    }

        .password-rules ul {
            margin: 8px 0 0;
            padding-left: 20px;
        }

        .password-rules li {
            color: #991b1b;
            margin-bottom: 4px;
        }

            .password-rules li.valid {
                color: #166534;
                font-weight: 700;
            }
</style>