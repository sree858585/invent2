<template>
    <footer class="site-footer">
        <div class="footer-content">
            <nav>
                <ul>

                    <img src="@/assets/logo.png" alt="New York State Logo" class="footer-logo" />

                    <li><a href="#">Agencies</a></li>
                    <li><a href="#">App Directory</a></li>
                    <li><a href="#">Counties</a></li>
                    <li><a href="#">Events</a></li>
                    <li><a href="#">Programs</a></li>
                    <li><a href="#">Services</a></li>
                </ul>
            </nav>
        </div>
    </footer>
</template>

<script>export default {
        name: "SiteFooter",
    };</script>

<style scoped>
    .site-footer {
        background-color: #000;
        color: #fff;
        padding: 20px 0;
        text-align: center;
    }

    .footer-content {
        display: flex;
        justify-content: center;
        align-items: center;
        flex-direction: column;
    }

    .footer-logo {
        height: 40px;
        margin-bottom: 10px;
    }

    nav ul {
        list-style: none;
        padding: 0;
        display: flex;
        gap: 15px;
    }

        nav ul li {
            display: inline;
        }

            nav ul li a {
                color: #fff;
                text-decoration: none;
                font-size: 14px;
            }

                nav ul li a:hover {
                    text-decoration: underline;
                }
</style>
