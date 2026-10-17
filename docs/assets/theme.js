// Apply the saved document theme before paint; file:// storage can be unavailable.
try {
  const theme = localStorage.getItem('sui-docs-theme');
  if (theme === 'light' || theme === 'dark') document.documentElement.dataset.theme = theme;
} catch { /* The system theme remains available through CSS. */ }
