<template>
    <div class="login-overlay">
        <div class="login-modal">
            <div class="login-header">
                <img src="@/assets/login1.gif" alt="Logo" class="login-logo" />
                <h2>🔐 Welcome Back</h2>
                <p> Please sign in to continue</p>
            </div>

            <form @submit.prevent="handleLogin" class="login-form">
                <div class="input-group">
                    <label for="email">Email</label>
                    <input type="email" v-model="email" placeholder="Enter your email" required />
                </div>

                <div class="input-group">
                    <label for="password">Password</label>
                    <input type="password" v-model="password" placeholder="Enter your password" required />
                </div>

                <button class="btn-login" type="submit">Login</button>

                <div class="extra-links">
                    <a href="#" @click.prevent="$emit('show-register')">Don't have an account? <strong>Register</strong></a>
                </div>
            </form>

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
                    if (!userData?.userId || typeof userData.userId !== "string") {
                        alert("Invalid user data received.");
                        return;
                    }

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
        },
    };</script>

<style scoped>
    .login-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.65);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 9999;
    }

    .login-modal {
        background: linear-gradient(to bottom right, #ffffff, #f7f8fc);
        border-radius: 20px;
        width: 100%;
        max-width: 520px;
        padding: 32px;
        position: relative;
        box-shadow: 0 20px 50px rgba(0, 0, 0, 0.2);
        animation: fadeIn 0.4s ease;
        font-family: 'Segoe UI', sans-serif;
        overflow: hidden;
    }

    .login-header {
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        margin-bottom: 24px;
    }

    .login-logo {
        width: 180px;
        height: auto;
        margin-bottom: 16px;
        border-radius: 12px;
    }

    .login-header h2 {
        font-size: 26px;
        font-weight: 700;
        color: #3f51b5;
        margin: 0 0 8px;
    }

    .login-header p {
        font-size: 15.5px;
        color: #555;
        margin: 0;
    }

    .input-group {
        margin-bottom: 18px;
        display: flex;
        flex-direction: column;
    }

        .input-group label {
            font-weight: 600;
            margin-bottom: 6px;
            color: #333;
        }

        .input-group input {
            padding: 12px 14px;
            border: 1px solid #ccc;
            border-radius: 10px;
            font-size: 16px;
            transition: border 0.3s ease;
            box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.05);
        }

            .input-group input:focus {
                border-color: #3f51b5;
                outline: none;
            }

    .btn-login {
        width: 100%;
        background-color: #3f51b5;
        color: white;
        padding: 12px;
        border: none;
        border-radius: 10px;
        font-size: 16px;
        font-weight: 600;
        cursor: pointer;
        transition: background-color 0.3s ease, box-shadow 0.3s ease;
        box-shadow: 0 3px 8px rgba(63, 81, 181, 0.3);
    }

        .btn-login:hover {
            background-color: #2c3e9f;
            box-shadow: 0 6px 14px rgba(63, 81, 181, 0.35);
        }

    .extra-links {
        text-align: center;
        margin-top: 20px;
    }

        .extra-links a {
            color: #3f51b5;
            text-decoration: underline;
            font-size: 14px;
        }

    .close-btn {
        position: absolute;
        top: 16px;
        right: 20px;
        font-size: 22px;
        border: none;
        background: transparent;
        cursor: pointer;
        color: #888;
        transition: color 0.3s ease;
    }

        .close-btn:hover {
            color: #333;
        }

    @keyframes fadeIn {
        from {
            opacity: 0;
            transform: translateY(-20px);
        }

        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
</style>