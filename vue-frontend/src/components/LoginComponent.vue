<template>
    <div class="login-overlay">
        <div class="login-modal">
            <div class="login-header">
                <img src="@/assets/login1.gif" alt="Logo" class="login-logo" />
                <h2>🔐 Welcome Back</h2>
                <p> Please sign in to continue</p>
            </div>

            <form v-if="!showForgotPassword" @submit.prevent="handleLogin" class="login-form">
                <div class="input-group">
                    <label>Email</label>
                    <input type="email" v-model="email" placeholder="Enter your email" required />
                </div>

                <div class="input-group">
                    <label>Password</label>
                    <input type="password" v-model="password" placeholder="Enter your password" required />
                </div>

                <div class="forgot-row">
                    <a href="#" @click.prevent="openForgotPassword">
                        Forgot Password?
                    </a>
                </div>

                <button class="btn-login" type="submit">Login</button>

                <div class="extra-links">
                    <a href="#" @click.prevent="$emit('show-register')">
                        Don't have an account? <strong>Register</strong>
                    </a>
                </div>
            </form>

            <div v-else class="forgot-box">
                <h3>Reset Password</h3>
                <p>Enter your email. We will send a password reset link.</p>

                <input type="email"
                       v-model="forgotEmail"
                       placeholder="Enter your email" />

                <button class="btn-login" @click="handleForgotPassword" :disabled="isSendingResetLink">
                    <span v-if="isSendingResetLink" class="spinner"></span>
                    <span v-if="isSendingResetLink">Sending Reset Link...</span>
                    <span v-else>Send Reset Link</span>
                </button>

                <button class="btn-secondary" @click="backToLogin">
                    Back to Login
                </button>

                <p v-if="resetMessage" class="reset-message">
                    {{ resetMessage }}
                </p>
            </div>

            <button class="close-btn" @click="$emit('close')">&times;</button>
        </div>
    </div>
</template>

<script>import apiClient from "@/axios.js";
import eventBus from "@/eventBus.js";

export default {
    data() {
        return {
            email: "",
            password: "",
            showForgotPassword: false,
            forgotEmail: "",
            resetMessage: "",
            resetLink: "",
            isSendingResetLink: false
        };
    },

    methods: {
        async handleLogin() {
            if (!this.email || !this.password) {
                alert("Please enter both email and password.");
                return;
            }

            try {
                const response = await apiClient.post("/login/login", {
                    email: this.email,
                    password: this.password,
                });

                const userData = response.data;

                localStorage.setItem("userId", userData.userId);
                localStorage.setItem("userName", `${userData.firstName} ${userData.lastName}`);
                localStorage.setItem("jwtToken", userData.token);
                localStorage.setItem("userRole", userData.role);

                eventBus.emit("auth-change");
                this.$emit("login-success", userData);
                this.$emit("close");
                setTimeout(() => window.location.reload(), 500);
            } catch (error) {
                alert(error.response?.data?.message || "Login failed. Please try again.");
            }
        },

        openForgotPassword() {
            this.forgotEmail = this.email;
            this.resetMessage = "";
            this.resetLink = "";
            this.showForgotPassword = true;
        },

        backToLogin() {
            this.showForgotPassword = false;
            this.resetMessage = "";
            this.resetLink = "";
            this.isSendingResetLink = false;
        },

        async handleForgotPassword() {
            if (!this.forgotEmail) {
                alert("Please enter your email.");
                return;
            }

            this.isSendingResetLink = true;
            this.resetMessage = "";

            try {
                await apiClient.post("/login/forgot-password", {
                    email: this.forgotEmail
                });

                this.resetMessage =
                    "Password reset link has been mailed to your email. Please check your inbox and follow the instructions to reset your password.";
            } catch (error) {
                alert(error.response?.data?.message || "Failed to send reset link.");
            } finally {
                this.isSendingResetLink = false;
            }
        }
    }
};</script>

<style scoped>
    .login-overlay {
        position: fixed;
        inset: 0;
        background: rgba(15, 23, 42, 0.65);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    }

    .login-modal {
        background: linear-gradient(180deg, #ffffff 0%, #faf7fd 100%);
        border-radius: 24px;
        width: 100%;
        max-width: 540px;
        padding: 36px 42px;
        position: relative;
        box-shadow: 0 24px 70px rgba(15, 23, 42, 0.28);
        font-family: 'Segoe UI', sans-serif;
        animation: fadeIn 0.35s ease;
    }

    .login-header {
        text-align: center;
        margin-bottom: 28px;
    }

    .login-logo {
        width: 155px;
        height: auto;
        margin-bottom: 14px;
    }

    .login-header h2 {
        font-size: 30px;
        font-weight: 800;
        color: #43285D;
        margin: 0 0 6px;
    }

    .login-header p {
        font-size: 16px;
        color: #5f5968;
        margin: 0;
    }

    .input-group {
        margin-bottom: 20px;
        display: flex;
        flex-direction: column;
    }

        .input-group label {
            font-size: 15px;
            font-weight: 700;
            margin-bottom: 8px;
            color: #2f2937;
        }

        .input-group input,
        .forgot-box input {
            padding: 14px 16px;
            border: 1px solid #d8d2df;
            border-radius: 14px;
            font-size: 16px;
            background: #ffffff;
            transition: 0.25s ease;
        }

            .input-group input:focus,
            .forgot-box input:focus {
                outline: none;
                border-color: #43285D;
                box-shadow: 0 0 0 4px rgba(67, 40, 93, 0.12);
            }

    .forgot-row {
        text-align: right;
        margin-top: -8px;
        margin-bottom: 18px;
    }

        .forgot-row a,
        .extra-links a {
            color: #43285D;
            font-size: 14px;
            font-weight: 600;
            text-decoration: underline;
        }

    .btn-login {
        width: 100%;
        background: #43285D;
        color: white;
        padding: 14px;
        border: none;
        border-radius: 14px;
        font-size: 17px;
        font-weight: 800;
        cursor: pointer;
        box-shadow: 0 10px 22px rgba(67, 40, 93, 0.28);
        transition: 0.25s ease;
    }

        .btn-login:hover {
            background: #361F4A;
            transform: translateY(-1px);
            box-shadow: 0 14px 28px rgba(67, 40, 93, 0.35);
        }

    .extra-links {
        text-align: center;
        margin-top: 22px;
    }

    .close-btn {
        position: absolute;
        top: 18px;
        right: 22px;
        font-size: 28px;
        border: none;
        background: transparent;
        cursor: pointer;
        color: #7a7480;
    }

        .close-btn:hover {
            color: #43285D;
        }

    .forgot-box {
        margin-top: 22px;
        padding: 20px;
        border-radius: 18px;
        background: #f4eff9;
        border: 1px solid #d8c8e6;
    }

        .forgot-box h3 {
            margin: 0 0 8px;
            color: #43285D;
            font-size: 22px;
        }

        .forgot-box p {
            color: #5f5968;
            margin-bottom: 12px;
        }

        .forgot-box input {
            width: 100%;
            box-sizing: border-box;
            margin-bottom: 14px;
        }

    .btn-secondary {
        width: 100%;
        margin-top: 10px;
        background: white;
        color: #43285D;
        padding: 13px;
        border: 1px solid #cbb8dd;
        border-radius: 14px;
        font-weight: 700;
        cursor: pointer;
    }

        .btn-secondary:hover {
            background: #faf7fd;
        }

    .reset-message {
        margin-top: 12px;
        color: #166534;
        font-weight: 700;
    }

    .reset-link {
        font-size: 13px;
        word-break: break-all;
    }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-18px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    .btn-login:disabled {
        opacity: 0.75;
        cursor: not-allowed;
        transform: none;
    }

    .spinner {
        display: inline-block;
        width: 16px;
        height: 16px;
        margin-right: 8px;
        border: 2px solid rgba(255, 255, 255, 0.45);
        border-top-color: #ffffff;
        border-radius: 50%;
        animation: spin 0.8s linear infinite;
        vertical-align: middle;
    }

    .reset-message {
        margin-top: 14px;
        padding: 12px 14px;
        background: #ecfdf3;
        border: 1px solid #bbf7d0;
        border-radius: 12px;
        color: #166534;
        font-weight: 700;
        line-height: 1.45;
    }

    @keyframes spin {
        to {
            transform: rotate(360deg);
        }
    }
</style>