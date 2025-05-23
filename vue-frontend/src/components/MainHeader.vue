<template>
    <header>
        <nav class="navbar navbar-custom" :style="bannerStyle">
            <div class="container navbar-container">
                <div class="navbar-brand">Department of Health</div>

                <div class="navbar-links">
                    <!-- If logged in, show profile dropdown -->
                    <div class="user-profile-wrapper"
                         v-if="isUserLoggedIn"
                         ref="profileWrapper">
                        <div class="user-avatar" @click="toggleDropdown">
                            <span class="user-name">Hello, {{ userName }}</span>
                            <img :src="profileImageUrl" alt="User" />
                        </div>
                        <transition name="fade">
                            <ul v-if="showDropdown" class="dropdown-menu">
                                <li @click="navigateToProfile">👤 Profile</li>
                                <li @click="handleLogout">🔓 Logout</li>
                            </ul>
                        </transition>
                    </div>

                    <!-- Else show login button -->
                    <button class="login-btn" v-else @click="navigateToLogin">
                        🔒 Login
                    </button>
                </div>
            </div>
        </nav>
    </header>
</template>

<script>import image from '@/assets/img.png';
    import defaultAvatar from '@/assets/profile.png';

    export default {
        name: 'MainHeader',
        data() {
            return {
                showDropdown: false,
                isUserLoggedIn: !!localStorage.getItem('jwtToken'),
                userName: localStorage.getItem('userName') || 'User',
                profileImageUrl: defaultAvatar,
            };
        },
        directives: {
            outside: {
                mounted(el, binding) {
                    el.clickOutsideEvent = function (event) {
                        if (!(el === event.target || el.contains(event.target))) {
                            binding.value();
                        }
                    };
                    document.body.addEventListener('click', el.clickOutsideEvent);
                },
                unmounted(el) {
                    document.body.removeEventListener('click', el.clickOutsideEvent);
                },
            },
        },
        computed: {
            bannerStyle() {
                return {
                    backgroundImage: `url(${image})`,
                    backgroundSize: 'cover',
                    backgroundPosition: 'center',
                    backgroundRepeat: 'no-repeat',
                };
            },
        },
        mounted() {
            document.addEventListener("click", this.handleClickOutside);
        },
        beforeUnmount() {
            document.removeEventListener("click", this.handleClickOutside);
        },
        methods: {
            toggleDropdown() {
                this.showDropdown = !this.showDropdown;
            },
            handleClickOutside(event) {
                const wrapper = this.$refs.profileWrapper;
                if (wrapper && !wrapper.contains(event.target)) {
                    this.showDropdown = false;
                }
            },
            closeDropdown() {
                this.showDropdown = false;
            },
            navigateToProfile() {
                const userId = localStorage.getItem('userId');
                if (!userId) return alert('User not found. Please login.');
                this.$router.push(`/profile/view/${userId}`);
                this.showDropdown = false;
            },
            handleLogout() {
                localStorage.clear();
                this.$router.push('/home');
                window.location.reload();
            },
            navigateToLogin() {
                this.$emit('show-login'); // Trigger the modal
            },
        },
    };</script>

<style scoped>
    .navbar-custom {
        background-color: #6e528d;
        color: white;
        padding: 15px 25px;
    }

    .navbar-container {
        display: flex;
        align-items: center;
        justify-content: space-between;
        width: 100%;
    }

    .navbar-brand {
        font-size: 20px;
        font-weight: bold;
        padding-left: 25px;
        color: white;
    }

    .navbar-links {
        display: flex;
        align-items: center;
    }

    .login-btn {
        background-color: transparent;
        color: white;
        border: none;
        cursor: pointer;
        font-size: 16px;
    }

    .user-profile-wrapper {
        position: relative;
        display: flex;
        align-items: center;
        cursor: pointer;
    }

    .user-avatar {
        display: flex;
        align-items: center;
        gap: 10px;
    }

        .user-avatar img {
            width: 40px;
            height: 40px;
            border-radius: 50%;
            object-fit: cover;
            border: 2px solid white;
        }

    .user-name {
        color: white;
        font-size: 16px;
        font-weight: 500;
    }

    .dropdown-menu {
        position: absolute;
        right: 0;
        top: 48px;
        background: white;
        color: black;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        list-style: none;
        padding: 10px 0;
        z-index: 999;
        min-width: 180px;
        transition: all 0.3s ease;
    }

        .dropdown-menu li {
            padding: 12px 20px;
            cursor: pointer;
            font-size: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

            .dropdown-menu li:hover {
                background-color: #f2f2f2;
                color: #6e528d;
            }

    .fade-enter-active,
    .fade-leave-active {
        transition: opacity 0.2s;
    }

    .fade-enter-from,
    .fade-leave-to {
        opacity: 0;
    }
</style>
