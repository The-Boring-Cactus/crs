<script setup>
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useSetupStore } from '@/store/setupStore';
import { toast } from 'vue-sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Database, UserPlus, Mail, ChevronRight, ChevronLeft,
  CheckCircle, Loader2, Server, Shield, Zap
} from 'lucide-vue-next';

const router = useRouter();
const setupStore = useSetupStore();

const currentStep = ref(1);
const totalSteps = 3;
const testingConnection = ref(false);
const connectionTested = ref(false);
const submitting = ref(false);

const dbConfig = ref({
  type: 'postgres',
  host: 'localhost',
  port: 5432,
  databaseName: '',
  username: '',
  password: ''
});

const adminConfig = ref({
  username: '',
  fullName: '',
  email: '',
  password: '',
  confirmPassword: ''
});

const smtpConfig = ref({
  host: '',
  port: 587,
  fromAddress: '',
  username: '',
  password: '',
  useSsl: true,
  isConfigured: false
});

const dbTypes = [
  { value: 'mssql', label: 'SQL Server', icon: '🟦', defaultPort: 1433 },
  { value: 'postgres', label: 'PostgreSQL', icon: '🐘', defaultPort: 5432 },
  { value: 'mysql', label: 'MySQL', icon: '🐬', defaultPort: 3306 }
];

const selectedDbType = computed(() => dbTypes.find(d => d.value === dbConfig.value.type));

function selectDbType(type) {
  dbConfig.value.type = type;
  dbConfig.value.port = dbTypes.find(d => d.value === type)?.defaultPort || 5432;
  connectionTested.value = false;
}

async function testConnection() {
  if (!dbConfig.value.host || !dbConfig.value.databaseName || !dbConfig.value.username) {
    toast.error('Missing fields', { description: 'Please fill in all required database fields' });
    return;
  }
  testingConnection.value = true;
  const result = await setupStore.testConnection(dbConfig.value);
  testingConnection.value = false;
  if (result.success || result.Success) {
    connectionTested.value = true;
    toast.success('Connection successful', { description: 'Database connection verified!' });
  } else {
    connectionTested.value = false;
    toast.error('Connection failed', { description: result.message || result.Message || 'Check credentials' });
  }
}

function canProceed() {
  if (currentStep.value === 1) {
    return connectionTested.value;
  }
  if (currentStep.value === 2) {
    return adminConfig.value.username && adminConfig.value.fullName &&
           adminConfig.value.email && adminConfig.value.password &&
           adminConfig.value.password === adminConfig.value.confirmPassword;
  }
  return true;
}

function nextStep() {
  if (currentStep.value < totalSteps && canProceed()) {
    currentStep.value++;
  }
}

function prevStep() {
  if (currentStep.value > 1) currentStep.value--;
}

async function completeSetup() {
  if (adminConfig.value.password !== adminConfig.value.confirmPassword) {
    toast.error('Password mismatch', { description: 'Passwords do not match' });
    return;
  }
  submitting.value = true;
  const setupData = {
    database: dbConfig.value,
    adminUser: {
      username: adminConfig.value.username,
      fullName: adminConfig.value.fullName,
      email: adminConfig.value.email
    },
    adminPassword: adminConfig.value.password,
    smtp: smtpConfig.value.host ? { ...smtpConfig.value, isConfigured: true } : null
  };

  const result = await setupStore.completeSetup(setupData);
  submitting.value = false;
  if (result.success || result.Success) {
    toast.success('Setup Complete!', { description: 'Redirecting to login...' });
    setTimeout(() => router.push('/auth/login'), 1500);
  } else {
    toast.error('Setup failed', { description: result.message || result.Message });
  }
}
</script>

<template>
  <div class="setup-page">
    <!-- Animated background -->
    <div class="setup-bg">
      <div class="bg-orb bg-orb-1"></div>
      <div class="bg-orb bg-orb-2"></div>
      <div class="bg-orb bg-orb-3"></div>
    </div>

    <div class="setup-container">
      <!-- Header -->
      <div class="setup-header">
        <div class="logo-area">
          <div class="logo-icon"><Zap class="w-7 h-7 text-white" /></div>
          <h1>CRS Reporter</h1>
        </div>
        <p class="setup-subtitle">Initial Setup Wizard</p>
      </div>

      <!-- Progress bar -->
      <div class="progress-track">
        <div v-for="s in totalSteps" :key="s" class="progress-step"
             :class="{ active: s === currentStep, completed: s < currentStep }">
          <div class="step-circle">
            <CheckCircle v-if="s < currentStep" class="w-4 h-4" />
            <span v-else>{{ s }}</span>
          </div>
          <span class="step-label">{{ ['Database', 'Admin User', 'SMTP'][s - 1] }}</span>
        </div>
        <div class="progress-line">
          <div class="progress-fill" :style="{ width: ((currentStep - 1) / (totalSteps - 1)) * 100 + '%' }"></div>
        </div>
      </div>

      <!-- Step 1: Database -->
      <div v-if="currentStep === 1" class="step-card">
        <div class="step-icon-header">
          <Database class="w-8 h-8 text-violet-400" />
          <div>
            <h2>Database Configuration</h2>
            <p class="text-muted">Select your database engine and configure the connection</p>
          </div>
        </div>

        <div class="db-type-selector">
          <div v-for="db in dbTypes" :key="db.value"
               class="db-type-card" :class="{ selected: dbConfig.type === db.value }"
               @click="selectDbType(db.value)">
            <span class="db-icon">{{ db.icon }}</span>
            <span class="db-label">{{ db.label }}</span>
          </div>
        </div>

        <div class="form-grid">
          <div class="form-group span-2">
            <Label>Host / Server</Label>
            <Input v-model="dbConfig.host" placeholder="localhost or IP address" />
          </div>
          <div class="form-group">
            <Label>Port</Label>
            <Input v-model.number="dbConfig.port" type="number" />
          </div>
          <div class="form-group">
            <Label>Database Name</Label>
            <Input v-model="dbConfig.databaseName" placeholder="crs_db" />
          </div>
          <div class="form-group">
            <Label>Username</Label>
            <Input v-model="dbConfig.username" placeholder="db_user" />
          </div>
          <div class="form-group">
            <Label>Password</Label>
            <Input v-model="dbConfig.password" type="password" placeholder="••••••••" />
          </div>
        </div>

        <div class="test-connection-area">
          <Button @click="testConnection" :disabled="testingConnection" variant="outline"
                  class="test-btn">
            <Loader2 v-if="testingConnection" class="w-4 h-4 animate-spin mr-2" />
            <Database v-else class="w-4 h-4 mr-2" />
            {{ testingConnection ? 'Testing...' : 'Test Connection' }}
          </Button>
          <div v-if="connectionTested" class="connection-success">
            <CheckCircle class="w-5 h-5 text-emerald-400" />
            <span>Connected successfully</span>
          </div>
        </div>
      </div>

      <!-- Step 2: Admin User -->
      <div v-if="currentStep === 2" class="step-card">
        <div class="step-icon-header">
          <Shield class="w-8 h-8 text-violet-400" />
          <div>
            <h2>Administrator Account</h2>
            <p class="text-muted">Create the initial admin user for the system</p>
          </div>
        </div>

        <div class="form-grid">
          <div class="form-group">
            <Label>Username</Label>
            <Input v-model="adminConfig.username" placeholder="admin" />
          </div>
          <div class="form-group">
            <Label>Full Name</Label>
            <Input v-model="adminConfig.fullName" placeholder="John Doe" />
          </div>
          <div class="form-group span-2">
            <Label>Email</Label>
            <Input v-model="adminConfig.email" type="email" placeholder="admin@company.com" />
          </div>
          <div class="form-group">
            <Label>Password</Label>
            <Input v-model="adminConfig.password" type="password" placeholder="••••••••" />
          </div>
          <div class="form-group">
            <Label>Confirm Password</Label>
            <Input v-model="adminConfig.confirmPassword" type="password" placeholder="••••••••" />
          </div>
        </div>

        <div v-if="adminConfig.password && adminConfig.confirmPassword && adminConfig.password !== adminConfig.confirmPassword"
             class="password-mismatch">
          Passwords do not match
        </div>
      </div>

      <!-- Step 3: SMTP -->
      <div v-if="currentStep === 3" class="step-card">
        <div class="step-icon-header">
          <Mail class="w-8 h-8 text-violet-400" />
          <div>
            <h2>Email Configuration</h2>
            <p class="text-muted">Optional — configure SMTP for email notifications</p>
          </div>
        </div>

        <div class="optional-badge">
          <span>Optional</span> — You can skip this step and configure it later
        </div>

        <div class="form-grid">
          <div class="form-group span-2">
            <Label>SMTP Host</Label>
            <Input v-model="smtpConfig.host" placeholder="smtp.gmail.com" />
          </div>
          <div class="form-group">
            <Label>Port</Label>
            <Input v-model.number="smtpConfig.port" type="number" />
          </div>
          <div class="form-group">
            <Label>Use SSL</Label>
            <div class="toggle-switch" @click="smtpConfig.useSsl = !smtpConfig.useSsl">
              <div class="toggle-track" :class="{ on: smtpConfig.useSsl }">
                <div class="toggle-thumb"></div>
              </div>
              <span>{{ smtpConfig.useSsl ? 'Enabled' : 'Disabled' }}</span>
            </div>
          </div>
          <div class="form-group span-2">
            <Label>From Address</Label>
            <Input v-model="smtpConfig.fromAddress" type="email" placeholder="noreply@company.com" />
          </div>
          <div class="form-group">
            <Label>Username</Label>
            <Input v-model="smtpConfig.username" placeholder="smtp_user" />
          </div>
          <div class="form-group">
            <Label>Password</Label>
            <Input v-model="smtpConfig.password" type="password" placeholder="••••••••" />
          </div>
        </div>
      </div>

      <!-- Navigation buttons -->
      <div class="step-actions">
        <Button v-if="currentStep > 1" variant="outline" @click="prevStep" class="nav-btn">
          <ChevronLeft class="w-4 h-4 mr-1" /> Back
        </Button>
        <div v-else></div>

        <Button v-if="currentStep < totalSteps" @click="nextStep" :disabled="!canProceed()" class="nav-btn primary-btn">
          Next <ChevronRight class="w-4 h-4 ml-1" />
        </Button>
        <Button v-else @click="completeSetup" :disabled="submitting" class="nav-btn finish-btn">
          <Loader2 v-if="submitting" class="w-4 h-4 animate-spin mr-2" />
          <CheckCircle v-else class="w-4 h-4 mr-2" />
          {{ submitting ? 'Setting up...' : 'Complete Setup' }}
        </Button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.setup-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0a0a0f;
  position: relative;
  overflow: hidden;
  padding: 2rem;
}

.setup-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
}

.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  opacity: 0.4;
  animation: float 20s ease-in-out infinite;
}
.bg-orb-1 { width: 500px; height: 500px; background: #7c3aed; top: -10%; left: -5%; animation-delay: 0s; }
.bg-orb-2 { width: 400px; height: 400px; background: #2563eb; bottom: -10%; right: -5%; animation-delay: -7s; }
.bg-orb-3 { width: 300px; height: 300px; background: #db2777; top: 50%; left: 50%; animation-delay: -14s; }

@keyframes float {
  0%, 100% { transform: translate(0, 0) scale(1); }
  33% { transform: translate(30px, -30px) scale(1.05); }
  66% { transform: translate(-20px, 20px) scale(0.95); }
}

.setup-container {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 680px;
}

.setup-header {
  text-align: center;
  margin-bottom: 2rem;
}
.logo-area { display: flex; align-items: center; justify-content: center; gap: 0.75rem; margin-bottom: 0.5rem; }
.logo-icon {
  width: 48px; height: 48px; border-radius: 14px;
  background: linear-gradient(135deg, #7c3aed, #2563eb);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 20px rgba(124, 58, 237, 0.4);
}
.setup-header h1 { font-size: 1.75rem; font-weight: 700; color: #f4f4f5; letter-spacing: -0.02em; }
.setup-subtitle { color: #71717a; font-size: 0.95rem; }

/* Progress */
.progress-track {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  position: relative;
  padding: 0 2rem;
}
.progress-line {
  position: absolute;
  top: 17px;
  left: calc(2rem + 17px);
  right: calc(2rem + 17px);
  height: 3px;
  background: #27272a;
  border-radius: 2px;
  z-index: 0;
}
.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #7c3aed, #2563eb);
  border-radius: 2px;
  transition: width 0.5s ease;
}
.progress-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  z-index: 1;
}
.step-circle {
  width: 34px; height: 34px; border-radius: 50%;
  background: #18181b; border: 2px solid #27272a;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.8rem; font-weight: 600; color: #71717a;
  transition: all 0.3s ease;
}
.progress-step.active .step-circle {
  border-color: #7c3aed;
  background: linear-gradient(135deg, #7c3aed, #6d28d9);
  color: white;
  box-shadow: 0 0 20px rgba(124, 58, 237, 0.4);
}
.progress-step.completed .step-circle {
  border-color: #10b981;
  background: #10b981;
  color: white;
}
.step-label { font-size: 0.75rem; color: #52525b; font-weight: 500; }
.progress-step.active .step-label { color: #a78bfa; }
.progress-step.completed .step-label { color: #6ee7b7; }

/* Step Card */
.step-card {
  background: rgba(24, 24, 27, 0.8);
  backdrop-filter: blur(20px);
  border: 1px solid #27272a;
  border-radius: 20px;
  padding: 2rem;
  animation: fadeIn 0.4s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.step-icon-header {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.75rem;
  padding-bottom: 1.25rem;
  border-bottom: 1px solid #27272a;
}
.step-icon-header h2 { font-size: 1.25rem; font-weight: 600; color: #f4f4f5; }
.text-muted { color: #71717a; font-size: 0.875rem; margin-top: 2px; }

/* DB Type Selector */
.db-type-selector {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}
.db-type-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1.25rem 0.75rem;
  border-radius: 14px;
  border: 2px solid #27272a;
  background: #18181b;
  cursor: pointer;
  transition: all 0.25s ease;
}
.db-type-card:hover { border-color: #3f3f46; background: #1f1f23; }
.db-type-card.selected {
  border-color: #7c3aed;
  background: rgba(124, 58, 237, 0.1);
  box-shadow: 0 0 20px rgba(124, 58, 237, 0.15);
}
.db-icon { font-size: 2rem; }
.db-label { font-size: 0.85rem; font-weight: 500; color: #a1a1aa; }
.db-type-card.selected .db-label { color: #c4b5fd; }

/* Form Grid */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}
.form-group { display: flex; flex-direction: column; gap: 0.4rem; }
.form-group.span-2 { grid-column: span 2; }
.form-group :deep(label) { font-size: 0.8rem; color: #a1a1aa; font-weight: 500; }
.form-group :deep(input) {
  background: #09090b; border: 1px solid #27272a; color: #f4f4f5;
  border-radius: 10px; padding: 0.6rem 0.75rem; font-size: 0.9rem;
  transition: border-color 0.2s;
}
.form-group :deep(input:focus) { border-color: #7c3aed; outline: none; box-shadow: 0 0 0 3px rgba(124,58,237,0.15); }
.form-group :deep(input::placeholder) { color: #3f3f46; }

/* Test Connection */
.test-connection-area {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-top: 1.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid #27272a;
}
.test-btn { border-color: #3f3f46 !important; color: #d4d4d8 !important; }
.connection-success {
  display: flex; align-items: center; gap: 0.5rem;
  color: #6ee7b7; font-size: 0.875rem; font-weight: 500;
}

/* Toggle Switch */
.toggle-switch {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  padding-top: 0.25rem;
}
.toggle-track {
  width: 44px; height: 24px; border-radius: 12px;
  background: #27272a;
  position: relative;
  transition: background 0.3s;
}
.toggle-track.on { background: #7c3aed; }
.toggle-thumb {
  width: 18px; height: 18px; border-radius: 50%;
  background: white;
  position: absolute;
  top: 3px; left: 3px;
  transition: transform 0.3s;
}
.toggle-track.on .toggle-thumb { transform: translateX(20px); }
.toggle-switch span { color: #a1a1aa; font-size: 0.85rem; }

/* Optional Badge */
.optional-badge {
  background: rgba(124, 58, 237, 0.08);
  border: 1px solid rgba(124, 58, 237, 0.2);
  border-radius: 10px;
  padding: 0.75rem 1rem;
  margin-bottom: 1.5rem;
  font-size: 0.85rem;
  color: #a1a1aa;
}
.optional-badge span {
  background: linear-gradient(135deg, #7c3aed, #2563eb);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  font-weight: 600;
}

/* Password mismatch */
.password-mismatch {
  margin-top: 0.75rem;
  color: #f87171;
  font-size: 0.85rem;
  font-weight: 500;
}

/* Navigation */
.step-actions {
  display: flex;
  justify-content: space-between;
  margin-top: 1.5rem;
}
.nav-btn { border-radius: 12px !important; padding: 0.6rem 1.5rem !important; font-weight: 500 !important; }
.primary-btn {
  background: linear-gradient(135deg, #7c3aed, #6d28d9) !important;
  border: none !important;
  color: white !important;
}
.primary-btn:hover { box-shadow: 0 4px 20px rgba(124,58,237,0.4); }
.primary-btn:disabled { opacity: 0.4; cursor: not-allowed; }
.finish-btn {
  background: linear-gradient(135deg, #10b981, #059669) !important;
  border: none !important;
  color: white !important;
}
.finish-btn:hover { box-shadow: 0 4px 20px rgba(16,185,129,0.4); }

@media (max-width: 640px) {
  .setup-page { padding: 1rem; }
  .form-grid { grid-template-columns: 1fr; }
  .form-group.span-2 { grid-column: span 1; }
  .db-type-selector { grid-template-columns: 1fr; }
  .step-card { padding: 1.25rem; }
  .progress-track { padding: 0 0.5rem; }
  .step-label { font-size: 0.65rem; }
}
</style>
