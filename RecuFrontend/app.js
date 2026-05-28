const API_BASE = '__API_BASE__';

const state = {
  token: localStorage.getItem('token') || '',
  user: JSON.parse(localStorage.getItem('user') || 'null'),
  campaignId: null,
  characterId: null,
  character: null,
};

const $ = (id) => document.getElementById(id);

function show(el, visible) {
  if (!el) return;
  el.classList.toggle('hidden', !visible);
}

function setText(el, text) {
  if (!el) return;
  el.textContent = text ?? '';
}

let toastTimer = null;
function toast(message) {
  const el = $('toast');
  if (!el) return;
  setText(el, message);
  show(el, true);
  if (toastTimer) clearTimeout(toastTimer);
  toastTimer = setTimeout(() => show(el, false), 2600);
}

async function api(path, options = {}) {
  const headers = { ...(options.headers || {}) };
  if (state.token) headers.Authorization = `Bearer ${state.token}`;
  if (!(options.body instanceof FormData)) headers['Content-Type'] = 'application/json';

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ message: res.statusText }));
    throw new Error(err.message || `Error ${res.status}`);
  }
  if (res.status === 204) return null;
  return res.json();
}

function showApp(loggedIn) {
  $('auth-section').classList.toggle('hidden', loggedIn);
  $('app-section').classList.toggle('hidden', !loggedIn);
  $('btn-logout').classList.toggle('hidden', !loggedIn);
  if (loggedIn && state.user) {
    setText($('user-info'), `${state.user.username} · ${state.user.role}`);
    loadCampaigns();
  }
}

async function login(username, password) {
  const data = await api('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, password }),
  });
  saveSession(data);
}

async function register(username, password, displayName) {
  const data = await api('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ username, password, displayName }),
  });
  saveSession(data);
}

function saveSession(data) {
  state.token = data.accessToken;
  state.user = { username: data.username, role: data.role, userId: data.userId };
  localStorage.setItem('token', state.token);
  localStorage.setItem('user', JSON.stringify(state.user));
  showApp(true);
}

function logout() {
  state.token = '';
  state.user = null;
  state.campaignId = null;
  state.characterId = null;
  state.character = null;
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  showApp(false);
}

async function loadCampaigns() {
  const list = $('campaign-list');
  const loading = $('campaigns-loading');
  const error = $('campaigns-error');
  show(error, false);
  show(loading, true);
  list.innerHTML = '';
  try {
    const campaigns = await api('/api/campaigns?sortBy=name&sortDir=asc');
    const items = (campaigns.value ?? campaigns);
    if (!items.length) {
      const li = document.createElement('li');
      li.textContent = 'No tienes campañas todavía. Crea la primera arriba.';
      li.style.cursor = 'default';
      list.appendChild(li);
      return;
    }
    items.forEach((c) => {
      const li = document.createElement('li');
      li.textContent = `${c.name} · ${c.setting}${c.isPublic ? ' · Pública' : ''}`;
      li.onclick = () => openCampaign(c);
      list.appendChild(li);
    });
  } catch (e) {
    setText(error, e.message);
    show(error, true);
  } finally {
    show(loading, false);
  }
}

function openCampaign(c) {
  state.campaignId = c.id;
  setText($('campaign-title'), c.name);
  show($('campaign-detail'), true);
  show($('character-detail'), false);
  loadCharacters();
}

async function loadCharacters() {
  const list = $('character-list');
  const loading = $('characters-loading');
  const error = $('characters-error');
  show(error, false);
  show(loading, true);
  list.innerHTML = '';
  try {
    const chars = await api(`/api/campaigns/${state.campaignId}/characters?sortBy=name`);
    const items = (chars.value ?? chars);
    if (!items.length) {
      const li = document.createElement('li');
      li.textContent = 'No hay personajes. Añade uno con el formulario.';
      li.style.cursor = 'default';
      list.appendChild(li);
      return;
    }
    items.forEach((ch) => {
      const li = document.createElement('li');
      li.textContent = `${ch.name} · ${ch.race} · ${ch.characterClass} · niv.${ch.level}${ch.isNpc ? ' · NPC' : ''}`;
      li.onclick = () => openCharacter(ch);
      list.appendChild(li);
    });
  } catch (e) {
    setText(error, e.message);
    show(error, true);
  } finally {
    show(loading, false);
  }
}

function openCharacter(ch) {
  state.characterId = ch.id;
  state.character = ch;
  setText($('character-title'), ch.name);
  show($('character-detail'), true);
  setText($('roll-result'), '');
  show($('roll-result'), false);
  setText($('spell-result'), '');
  show($('spell-result'), false);
  renderSheet(ch);
  loadAttachments();
}

function abilityMod(score) {
  const n = Number(score);
  if (Number.isNaN(n)) return '—';
  const m = Math.floor((n - 10) / 2);
  return m >= 0 ? `+${m}` : `${m}`;
}

function renderSheet(ch) {
  const sheet = $('sheet');
  if (!sheet) return;
  const safe = (v, fallback = '—') => (v === null || v === undefined || v === '' ? fallback : v);

  const size = safe(ch.size);
  const speed = safe(ch.speedFeet);
  const init = safe(ch.initiative);

  sheet.innerHTML = `
    <div class="sheet__panel">
      <div class="kpi">
        <div class="kpi__item"><div class="kpi__label">CA</div><div class="kpi__value">${safe(ch.armorClass, 0)}</div></div>
        <div class="kpi__item"><div class="kpi__label">PG</div><div class="kpi__value">${safe(ch.hitPoints, 0)}</div></div>
        <div class="kpi__item"><div class="kpi__label">Comp.</div><div class="kpi__value">${safe(ch.proficiencyBonus, 0)}</div></div>
      </div>
      <div style="height:12px"></div>
      <div class="kpi">
        <div class="kpi__item"><div class="kpi__label">Iniciativa</div><div class="kpi__value">${init}</div></div>
        <div class="kpi__item"><div class="kpi__label">Velocidad</div><div class="kpi__value">${speed} ft</div></div>
        <div class="kpi__item"><div class="kpi__label">Tamaño</div><div class="kpi__value">${size}</div></div>
      </div>
    </div>
    <div class="sheet__panel">
      <div class="stats">
        ${statBox('FUE', ch.strength)}
        ${statBox('DES', ch.dexterity)}
        ${statBox('CON', ch.constitution)}
        ${statBox('INT', ch.intelligence)}
        ${statBox('SAB', ch.wisdom)}
        ${statBox('CAR', ch.charisma)}
      </div>
    </div>
  `;
}

function statBox(abbr, score) {
  const s = (score === null || score === undefined) ? '—' : score;
  const mod = (score === null || score === undefined) ? '—' : abilityMod(score);
  return `
    <div class="stat">
      <div class="stat__top">
        <div class="stat__abbr">${abbr}</div>
        <div class="stat__mod">${mod}</div>
      </div>
      <div class="stat__score">${s}</div>
    </div>
  `;
}

async function loadAttachments() {
  const items = await api(`/api/characters/${state.characterId}/attachments`);
  const list = $('attachment-list');
  list.innerHTML = '';
  (items.value ?? items).forEach((a) => {
    const li = document.createElement('li');
    li.textContent = `${a.fileName} (${a.sizeBytes} bytes)`;
    list.appendChild(li);
  });
}

// Events
document.querySelectorAll('[data-tab]').forEach((btn) => {
  btn.onclick = () => {
    document.querySelectorAll('[data-tab]').forEach((b) => b.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('[data-tab]').forEach((b) => b.setAttribute('aria-selected', String(b === btn)));
    $('login-form').classList.toggle('hidden', btn.dataset.tab !== 'login');
    $('register-form').classList.toggle('hidden', btn.dataset.tab !== 'register');
  };
});

$('login-form').onsubmit = async (e) => {
  e.preventDefault();
  const errEl = $('auth-error');
  setText(errEl, '');
  show(errEl, false);
  try {
    const fd = new FormData(e.target);
    await login(fd.get('username'), fd.get('password'));
    toast('Sesión iniciada');
  } catch (err) {
    setText(errEl, err.message);
    show(errEl, true);
  }
};

$('register-form').onsubmit = async (e) => {
  e.preventDefault();
  const errEl = $('auth-error');
  setText(errEl, '');
  show(errEl, false);
  try {
    const fd = new FormData(e.target);
    await register(fd.get('username'), fd.get('password'), fd.get('displayName'));
    toast('Usuario creado');
  } catch (err) {
    setText(errEl, err.message);
    show(errEl, true);
  }
};

$('btn-logout').onclick = logout;

$('campaign-form').onsubmit = async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  try {
    await api('/api/campaigns', {
      method: 'POST',
      body: JSON.stringify({
        name: fd.get('name'),
        setting: fd.get('setting'),
        description: '',
        isPublic: fd.get('isPublic') === 'on',
        isActive: true,
      }),
    });
    e.target.reset();
    toast('Campaña creada');
    loadCampaigns();
  } catch (e2) {
    toast(e2.message);
  }
};

$('character-form').onsubmit = async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  try {
    await api(`/api/campaigns/${state.campaignId}/characters`, {
      method: 'POST',
      body: JSON.stringify({
        name: fd.get('name'),
        race: fd.get('race'),
        characterClass: fd.get('characterClass'),
        level: Number(fd.get('level')),
        armorClass: Number(fd.get('armorClass')),
        hitPoints: Number(fd.get('hitPoints')),
        proficiencyBonus: Number(fd.get('proficiencyBonus')),
        strength: Number(fd.get('strength')),
        dexterity: Number(fd.get('dexterity')),
        constitution: Number(fd.get('constitution')),
        intelligence: Number(fd.get('intelligence')),
        wisdom: Number(fd.get('wisdom')),
        charisma: Number(fd.get('charisma')),
        initiative: Number(fd.get('initiative')),
        speedFeet: Number(fd.get('speedFeet')),
        size: String(fd.get('size') || 'Medium'),
        isNpc: fd.get('isNpc') === 'on',
      }),
    });
    e.target.reset();
    toast('Personaje creado');
    loadCharacters();
  } catch (e2) {
    toast(e2.message);
  }
};

$('btn-roll').onclick = async () => {
  const expr = $('dice-expression').value;
  const mode = Number($('d20-mode').value);
  try {
    const roll = await api(`/api/characters/${state.characterId}/rolls`, {
      method: 'POST',
      body: JSON.stringify({ label: 'Tirada web', diceExpression: expr, d20Mode: mode }),
    });
    const msg = `Total: ${roll.total} (${roll.rawResults})${roll.isCritical ? ' · ¡CRÍTICO!' : ''}`;
    setText($('roll-result'), msg);
    show($('roll-result'), true);
  } catch (e) {
    toast(e.message);
  }
};

$('btn-spell').onclick = async () => {
  const name = $('spell-name').value;
  try {
    const spell = await api(`/api/public/dnd/spells?name=${encodeURIComponent(name)}`);
    setText($('spell-result'), JSON.stringify(spell, null, 2));
    show($('spell-result'), true);
  } catch (e) {
    toast(e.message);
  }
};

$('btn-upload').onclick = async () => {
  const file = $('file-input').files[0];
  if (!file) return toast('Selecciona un archivo');
  const fd = new FormData();
  fd.append('file', file);
  try {
    await api(`/api/characters/${state.characterId}/attachments`, { method: 'POST', body: fd, headers: {} });
    toast('Adjunto subido');
    loadAttachments();
  } catch (e) {
    toast(e.message);
  }
};

$('btn-back-campaigns').onclick = () => {
  show($('campaign-detail'), false);
  show($('character-detail'), false);
};

$('btn-back-characters').onclick = () => show($('character-detail'), false);

if (state.token) showApp(true);
else showApp(false);
