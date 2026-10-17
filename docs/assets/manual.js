(() => {
  'use strict';
  const $ = (selector, root = document) => root.querySelector(selector);
  const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];
  const normalize = text => text.normalize('NFKC').toLocaleLowerCase().replace(/\s+/g, ' ').trim();
  const titleOf = heading => heading.textContent.replace(/#$/, '').trim();
  const navLinks = $$('.sidebar .nav-link');
  const sections = [$('#概览'), ...$$('.doc-section')];
  const sidebar = $('#sidebar');
  const menu = $('.menu-toggle');
  const toc = $('#chapter-toc');
  const themeButton = $('.theme-toggle');
  const systemTheme = matchMedia('(prefers-color-scheme: dark)');
  let currentSection;
  let scrollPending = false;
  let toastTimer;

  sidebar.classList.add('js-ready');
  menu.hidden = false;
  menu.addEventListener('click', () => {
    const open = menu.getAttribute('aria-expanded') !== 'true';
    menu.setAttribute('aria-expanded', String(open));
    sidebar.classList.toggle('is-open', open);
  });
  const closeMenu = () => {
    menu.setAttribute('aria-expanded', 'false');
    sidebar.classList.remove('is-open');
  };
  navLinks.forEach(link => link.addEventListener('click', closeMenu));
  document.addEventListener('click', event => {
    if (!sidebar.contains(event.target) && !menu.contains(event.target)) closeMenu();
  });

  function updateThemeLabel() {
    const dark = document.documentElement.dataset.theme === 'dark' ||
      (!document.documentElement.dataset.theme && systemTheme.matches);
    themeButton.setAttribute('aria-pressed', String(dark));
    themeButton.setAttribute('aria-label', `切换为${dark ? '浅色' : '深色'}文档主题`);
    themeButton.title = themeButton.getAttribute('aria-label');
  }
  themeButton.hidden = false;
  updateThemeLabel();
  systemTheme.addEventListener('change', updateThemeLabel);
  themeButton.addEventListener('click', () => {
    const theme = themeButton.getAttribute('aria-pressed') === 'true' ? 'light' : 'dark';
    document.documentElement.dataset.theme = theme;
    try { localStorage.setItem('sui-docs-theme', theme); } catch { /* Keep the current session usable. */ }
    updateThemeLabel();
  });

  function updateReadingPosition() {
    scrollPending = false;
    const threshold = innerWidth <= 700 ? 140 : 125;
    let active = sections[0];
    for (const section of sections) {
      if (section.getBoundingClientRect().top <= threshold) active = section;
      else break;
    }
    const activeId = active.id || $('h2', active).id;
    if (active !== currentSection) {
      currentSection = active;
      navLinks.forEach(link => {
        if (decodeURIComponent(link.hash.slice(1)) === activeId) link.setAttribute('aria-current', 'location');
        else link.removeAttribute('aria-current');
      });
      toc.replaceChildren();
      const headings = active.id === '概览' ? [$('#hero-title')] : $$('h2, h3, h4', active);
      headings.forEach(heading => {
        const link = document.createElement('a');
        link.href = `#${heading.id}`;
        link.textContent = titleOf(heading);
        toc.append(link);
      });
    }
    let activeToc = toc.firstElementChild;
    $$('a', toc).forEach(link => {
      const target = document.getElementById(decodeURIComponent(link.hash.slice(1)));
      if (target?.getBoundingClientRect().top <= threshold + 10) activeToc = link;
      link.removeAttribute('aria-current');
    });
    activeToc?.setAttribute('aria-current', 'location');
  }
  function scheduleReadingUpdate() {
    if (scrollPending) return;
    scrollPending = true;
    requestAnimationFrame(updateReadingPosition);
  }
  addEventListener('scroll', scheduleReadingUpdate, { passive: true });
  addEventListener('resize', scheduleReadingUpdate);
  addEventListener('hashchange', scheduleReadingUpdate);
  addEventListener('load', scheduleReadingUpdate);
  updateReadingPosition();

  function showToast(message) {
    clearTimeout(toastTimer);
    $('#copy-status').textContent = message;
    toastTimer = setTimeout(() => { $('#copy-status').textContent = ''; }, 3500);
  }
  $$('.copy-button').forEach(button => {
    button.hidden = false;
    button.addEventListener('click', async () => {
      const code = $('code', button.closest('.code-block'));
      try {
        if (!navigator.clipboard?.writeText) throw new Error('clipboard unavailable');
        await navigator.clipboard.writeText(code.textContent);
        button.textContent = '已复制 ✓';
        showToast('代码已复制');
        setTimeout(() => { button.textContent = '复制代码'; }, 1800);
      } catch {
        const range = document.createRange();
        range.selectNodeContents(code);
        const selection = getSelection();
        selection.removeAllRanges();
        selection.addRange(range);
        showToast(`代码已选中，请按 ${/Mac/.test(navigator.platform) ? '⌘' : 'Ctrl'}+C 复制`);
      }
    });
  });

  const dialog = $('#search-dialog');
  const input = $('#search-input');
  const results = $('#search-results');
  const summary = $('#search-summary');
  let selectedIndex = 0;
  let returnFocus;
  const searchEntries = [];
  $$('.doc-section').forEach(section => {
    const chapter = titleOf($('h2', section));
    $$('h2, h3, h4', section).forEach(heading => {
      const parts = [];
      let sibling = heading.nextElementSibling;
      while (sibling && !/^H[234]$/.test(sibling.tagName)) {
        parts.push(sibling.textContent.replace(/复制代码/g, ''));
        sibling = sibling.nextElementSibling;
      }
      const title = titleOf(heading);
      const text = parts.join(' ').replace(/\s+/g, ' ').trim();
      searchEntries.push({ id: heading.id, title, chapter, text, titleKey: normalize(title), key: normalize(`${chapter} ${title} ${text}`) });
    });
  });

  function markText(parent, text, terms) {
    if (!terms.length) { parent.textContent = text; return; }
    const escapedTerms = terms.map(term => term.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'));
    const pattern = new RegExp(`(${escapedTerms.join('|')})`, 'gi');
    // Always create text nodes: queries and document text never become executable HTML.
    text.split(pattern).forEach((part, index) => {
      const element = document.createElement(index % 2 ? 'mark' : 'span');
      element.textContent = part;
      parent.append(element);
    });
  }
  function selectResult(index, scroll = false) {
    const links = $$('a', results);
    if (!links.length) return;
    selectedIndex = (index + links.length) % links.length;
    links.forEach((link, i) => link.classList.toggle('is-selected', i === selectedIndex));
    if (scroll) links[selectedIndex].scrollIntoView({ block: 'nearest' });
  }
  function renderSearch() {
    const query = normalize(input.value).slice(0, 200);
    const terms = query.split(' ').filter(Boolean);
    const matches = terms.length ? searchEntries.filter(entry => terms.every(term => entry.key.includes(term)))
      .sort((a, b) => Number(b.titleKey.includes(query)) - Number(a.titleKey.includes(query))) :
      searchEntries.filter(entry => ['快速开始', 'suicard-卡片控件', 'suinavigationview-导航控件', '主题令牌', '常见问题'].includes(entry.id));
    summary.textContent = terms.length ? (matches.length ? `找到 ${matches.length} 个结果${matches.length > 40 ? '，显示前 40 个；可增加关键词缩小范围' : ''}` : '没有找到匹配内容') : '从常用章节开始，或搜索全部正文与代码';
    results.replaceChildren();
    matches.slice(0, 40).forEach(entry => {
      const link = document.createElement('a');
      link.className = 'search-result';
      link.href = `#${entry.id}`;
      const chapter = document.createElement('small');
      chapter.textContent = entry.chapter;
      const title = document.createElement('strong');
      markText(title, entry.title, terms);
      const snippet = document.createElement('p');
      const hitIndex = terms.length ? normalize(entry.text).indexOf(terms[0]) : 0;
      const start = Math.max(0, hitIndex - 30);
      const excerpt = (start > 0 ? '…' : '') + entry.text.slice(start, start + 130) + (entry.text.length > start + 130 ? '…' : '');
      markText(snippet, excerpt, terms);
      link.append(chapter, title, snippet);
      link.addEventListener('click', () => {
        dialog.close();
        closeMenu();
        requestAnimationFrame(() => {
          const heading = document.getElementById(entry.id);
          heading.setAttribute('tabindex', '-1');
          heading.focus({ preventScroll: true });
          heading.scrollIntoView({ block: 'start' });
        });
      });
      results.append(link);
    });
    if (!matches.length) {
      const empty = document.createElement('p');
      empty.className = 'empty-results';
      empty.textContent = '试试“按钮”“导航”“SUI.Brush”或“DataGrid”。也可以输入代码中的属性名。';
      results.append(empty);
    }
    results.scrollTop = 0;
    selectResult(0);
  }
  $$('[data-search-open]').forEach(button => {
    button.hidden = false;
    button.addEventListener('click', openSearch);
  });
  function openSearch() {
    if (dialog.open) { input.focus(); return; }
    returnFocus = document.activeElement;
    renderSearch();
    dialog.showModal();
    input.focus();
    input.select();
  }
  input.addEventListener('input', renderSearch);
  input.addEventListener('keydown', event => {
    if (event.isComposing) return;
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault();
      selectResult(selectedIndex + (event.key === 'ArrowDown' ? 1 : -1), true);
    } else if (event.key === 'Enter') {
      event.preventDefault();
      $$('a', results)[selectedIndex]?.click();
    }
  });
  dialog.addEventListener('click', event => {
    if (event.target !== dialog) return;
    const rect = dialog.getBoundingClientRect();
    if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) dialog.close();
  });
  dialog.addEventListener('close', () => { returnFocus?.focus({ preventScroll: true }); });
  document.addEventListener('keydown', event => {
    if (event.isComposing) return;
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault();
      openSearch();
    } else if (event.key === 'Escape' && sidebar.classList.contains('is-open')) {
      closeMenu();
      menu.focus();
    }
  });
})();
