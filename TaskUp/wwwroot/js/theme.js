document.addEventListener('DOMContentLoaded', function () {
    const savedTheme = localStorage.getItem('theme');
    const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';

    if (savedTheme && savedTheme !== currentTheme) {
        document.documentElement.classList.remove('dark', 'light');
        document.documentElement.classList.add(savedTheme);
    }

    const icon = document.getElementById('themeIcon');
    if (icon) {
        const theme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        if (theme === 'light') {
            icon.classList.remove('fa-sun');
            icon.classList.add('fa-moon');
        } else {
            icon.classList.remove('fa-moon');
            icon.classList.add('fa-sun');
        }
    }
});