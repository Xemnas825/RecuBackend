/** Reglas D&D 5e (PHB) para fichas y tiradas en el cliente. */
const DND_SKILLS = [
  { id: 'athletics',       name: 'Atletismo',           ability: 'strength' },
  { id: 'acrobatics',      name: 'Acrobacias',          ability: 'dexterity' },
  { id: 'sleightOfHand',   name: 'Juego de manos',      ability: 'dexterity' },
  { id: 'stealth',         name: 'Sigilo',              ability: 'dexterity' },
  { id: 'arcana',          name: 'Arcanos',             ability: 'intelligence' },
  { id: 'history',         name: 'Historia',            ability: 'intelligence' },
  { id: 'investigation',   name: 'Investigación',       ability: 'intelligence' },
  { id: 'nature',          name: 'Naturaleza',          ability: 'intelligence' },
  { id: 'religion',        name: 'Religión',            ability: 'intelligence' },
  { id: 'animalHandling',  name: 'Trato con animales',  ability: 'wisdom' },
  { id: 'insight',         name: 'Perspicacia',         ability: 'wisdom' },
  { id: 'medicine',        name: 'Medicina',            ability: 'wisdom' },
  { id: 'perception',      name: 'Percepción',          ability: 'wisdom' },
  { id: 'survival',        name: 'Supervivencia',       ability: 'wisdom' },
  { id: 'deception',       name: 'Engaño',              ability: 'charisma' },
  { id: 'intimidation',    name: 'Intimidación',        ability: 'charisma' },
  { id: 'performance',     name: 'Interpretación',      ability: 'charisma' },
  { id: 'persuasion',      name: 'Persuasión',          ability: 'charisma' },
];

const DND_ABILITIES = [
  { key: 'strength',     label: 'FUE', name: 'Fuerza' },
  { key: 'dexterity',    label: 'DES', name: 'Destreza' },
  { key: 'constitution', label: 'CON', name: 'Constitución' },
  { key: 'intelligence', label: 'INT', name: 'Inteligencia' },
  { key: 'wisdom',       label: 'SAB', name: 'Sabiduría' },
  { key: 'charisma',     label: 'CAR', name: 'Carisma' },
];

const DND_ALIGNMENTS = [
  'Legal Bueno', 'Neutral Bueno', 'Caótico Bueno',
  'Legal Neutral', 'Neutral', 'Caótico Neutral',
  'Legal Malvado', 'Neutral Malvado', 'Caótico Malvado',
];

function proficiencyFromLevel(level) {
  const lv = Math.max(1, Math.min(20, Number(level) || 1));
  return 2 + Math.floor((lv - 1) / 4);
}

function abilityModValue(score) {
  return Math.floor((Number(score) - 10) / 2);
}

function formatMod(mod) {
  return mod >= 0 ? `+${mod}` : `${mod}`;
}

function normalizeCharacter(ch) {
  if (!ch) return ch;
  return {
    ...ch,
    constitution: ch.constitution ?? 10,
    intelligence: ch.intelligence ?? 10,
    wisdom: ch.wisdom ?? 10,
    charisma: ch.charisma ?? 10,
    currentHitPoints: ch.currentHitPoints ?? ch.hitPoints ?? 0,
    alignment: ch.alignment ?? '',
    background: ch.background ?? '',
    languages: ch.languages ?? '',
    skillProficiencies: Array.isArray(ch.skillProficiencies) ? ch.skillProficiencies : [],
  };
}

function isSkillProficient(ch, skillId) {
  const list = ch.skillProficiencies || [];
  return list.includes(skillId);
}

function skillModifier(ch, skillId) {
  const skill = DND_SKILLS.find((s) => s.id === skillId);
  if (!skill) return 0;
  const ability = abilityModValue(ch[skill.ability] ?? 10);
  const prof = isSkillProficient(ch, skillId) ? (ch.proficiencyBonus ?? 2) : 0;
  return ability + prof;
}

function savingThrowModifier(ch, abilityKey) {
  return abilityModValue(ch[abilityKey] ?? 10);
}

function buildD20Expression(modifier) {
  const m = Number(modifier) || 0;
  if (m === 0) return '1d20';
  return m > 0 ? `1d20+${m}` : `1d20${m}`;
}

function characterPayload(data, form) {
  const level = Number(data.level) || 1;
  const maxHp = Number(data.hitPoints) || 0;
  let currentHp = Number(data.currentHitPoints);
  if (Number.isNaN(currentHp)) currentHp = maxHp;

  return {
    name: data.name,
    race: data.race,
    characterClass: data.characterClass,
    level,
    armorClass: Number(data.armorClass) || 10,
    hitPoints: maxHp,
    currentHitPoints: Math.min(currentHp, maxHp),
    proficiencyBonus: Number(data.proficiencyBonus) || proficiencyFromLevel(level),
    strength: Number(data.strength) || 10,
    dexterity: Number(data.dexterity) || 10,
    constitution: Number(data.constitution) || 10,
    intelligence: Number(data.intelligence) || 10,
    wisdom: Number(data.wisdom) || 10,
    charisma: Number(data.charisma) || 10,
    alignment: data.alignment || '',
    background: data.background || '',
    languages: data.languages || '',
    skillProficiencies: readSkillProficiencies(form),
    isNpc: !!data.isNpc,
  };
}

function readSkillProficiencies(form) {
  if (!form) return [];
  return [...form.querySelectorAll('input[data-skill]:checked')].map((el) => el.dataset.skill);
}

function mountSkillProficiencies(container, selected = []) {
  if (!container) return;
  const set = new Set(selected);
  const byAbility = {};
  DND_ABILITIES.forEach((a) => { byAbility[a.key] = []; });
  DND_SKILLS.forEach((s) => byAbility[s.ability].push(s));

  let html = '<div class="skill-profs-grid">';
  DND_ABILITIES.forEach((ab) => {
    const skills = byAbility[ab.key];
    if (!skills.length) return;
    html += `<div class="skill-profs-group"><div class="skill-profs-group__title">${ab.name} (${ab.label})</div><div class="skill-profs-group__list">`;
    skills.forEach((s) => {
      const checked = set.has(s.id) ? 'checked' : '';
      html += `
        <label class="skill-prof-check">
          <input type="checkbox" data-skill="${s.id}" ${checked} />
          <span>${s.name}</span>
        </label>`;
    });
    html += '</div></div>';
  });
  html += '</div>';
  container.innerHTML = html;
}

function bindLevelProficiencySync(form) {
  if (!form) return;
  const levelInput = form.querySelector('[name="level"]');
  const pbInput = form.querySelector('[name="proficiencyBonus"]');
  if (!levelInput || !pbInput) return;
  levelInput.addEventListener('change', () => {
    pbInput.value = proficiencyFromLevel(levelInput.value);
  });
}
