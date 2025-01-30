<template>
    <header class="site-header">
        <div class="header-content">
            <img src="@/assets/logo.png" alt="New York State Logo" class="logo" />
            <p>An official website of New York State. <a href="#">Here's how you know</a></p>
        </div>
    </header>
</template>

<script>export default {
        name: "SiteHeader",
    };</script>

<style scoped>
    .site-header {
        background-color: #ffffff;
        padding: 10px 20px;
        display: flex;
        
        border-bottom: 1px solid #ccc;
    }

    .header-content {
        display: flex;
        align-items: center;
    }

    .logo {
        height: 40px;
        margin-right: 10px;
    }

    p {
        font-size: 14px;
        color: #333;
    }

    a {
        color: #0056b3;
        text-decoration: underline;
    }
</style>
