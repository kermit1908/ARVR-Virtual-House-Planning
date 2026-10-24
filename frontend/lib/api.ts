import {
  AuthResponse,
  User,
  Project,
  BIMHouseConfig,
  BIMModelResponse,
  QTOResponse,
  BOQResponse,
  CostEstimateResponse,
  AIPredictResponse,
  ModelComparisonResponse,
  OptimizationResponse,
  ScheduleResponse,
  ProjectReportResponse
} from '../types';

const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api';

class ApiClient {
  private getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('smartbuild_token');
    }
    return null;
  }

  public setToken(token: string) {
    if (typeof window !== 'undefined') {
      localStorage.setItem('smartbuild_token', token);
    }
  }

  public clearToken() {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('smartbuild_token');
      localStorage.removeItem('smartbuild_user');
    }
  }

  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const token = this.getToken();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const res = await fetch(`${API_BASE}${endpoint}`, {
      ...options,
      headers,
    });

    if (!res.ok) {
      let errorMsg = `API Error ${res.status}: ${res.statusText}`;
      try {
        const errorData = await res.json();
        if (errorData.detail) errorMsg = errorData.detail;
      } catch {
        // use default errorMsg
      }
      throw new Error(errorMsg);
    }

    if (res.status === 204) {
      return {} as T;
    }

    return res.json();
  }

  // --- Auth ---
  async signup(email: string, password: string, fullName: string): Promise<AuthResponse> {
    const data = await this.request<AuthResponse>('/auth/signup', {
      method: 'POST',
      body: JSON.stringify({ email, password, full_name: fullName }),
    });
    this.setToken(data.access_token);
    if (typeof window !== 'undefined') {
      localStorage.setItem('smartbuild_user', JSON.stringify(data.user));
    }
    return data;
  }

  async login(email: string, password: string): Promise<AuthResponse> {
    const data = await this.request<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    this.setToken(data.access_token);
    if (typeof window !== 'undefined') {
      localStorage.setItem('smartbuild_user', JSON.stringify(data.user));
    }
    return data;
  }

  async getMe(): Promise<User> {
    return this.request<User>('/auth/me');
  }

  logout() {
    this.clearToken();
  }

  // --- Projects ---
  async createProject(name: string, location: string, plotWidth: number, plotLength: number, description?: string): Promise<Project> {
    return this.request<Project>('/projects', {
      method: 'POST',
      body: JSON.stringify({
        name,
        location,
        plot_width: plotWidth,
        plot_length: plotLength,
        description: description || ''
      }),
    });
  }

  async listProjects(): Promise<Project[]> {
    return this.request<Project[]>('/projects');
  }

  async getProject(projectId: string): Promise<Project> {
    return this.request<Project>(`/projects/${projectId}`);
  }

  async updateProject(projectId: string, updates: Partial<Project>): Promise<Project> {
    return this.request<Project>(`/projects/${projectId}`, {
      method: 'PUT',
      body: JSON.stringify(updates),
    });
  }

  async deleteProject(projectId: string): Promise<void> {
    await this.request<void>(`/projects/${projectId}`, {
      method: 'DELETE',
    });
  }

  // --- BIM ---
  async generateHouse(projectId: string, config: BIMHouseConfig): Promise<BIMModelResponse> {
    return this.request<BIMModelResponse>(`/projects/${projectId}/generate-house`, {
      method: 'POST',
      body: JSON.stringify(config),
    });
  }

  async getBIMModel(projectId: string): Promise<BIMModelResponse> {
    return this.request<BIMModelResponse>(`/projects/${projectId}/bim`);
  }

  // --- QTO & BOQ ---
  async getQuantities(projectId: string): Promise<QTOResponse> {
    return this.request<QTOResponse>(`/projects/${projectId}/quantities`);
  }

  async getBOQ(projectId: string): Promise<BOQResponse> {
    return this.request<BOQResponse>(`/projects/${projectId}/boq`);
  }

  // --- 5D Cost ---
  async getCostEstimate(projectId: string): Promise<CostEstimateResponse> {
    return this.request<CostEstimateResponse>(`/projects/${projectId}/cost-estimate`);
  }

  async listRates(location: string = 'Bengaluru'): Promise<any[]> {
    return this.request<any[]>(`/rates?location=${encodeURIComponent(location)}`);
  }

  // --- AI Prediction & Optimization ---
  async predictCost(projectId: string): Promise<AIPredictResponse> {
    return this.request<AIPredictResponse>(`/projects/${projectId}/ai-predict`, {
      method: 'POST',
    });
  }

  async getModelsComparison(): Promise<ModelComparisonResponse> {
    return this.request<ModelComparisonResponse>('/ai/models-comparison');
  }

  async getOptimizations(projectId: string): Promise<OptimizationResponse> {
    return this.request<OptimizationResponse>(`/projects/${projectId}/optimize-cost`);
  }

  // --- 4D Schedule ---
  async getSchedule(projectId: string, startDate?: string): Promise<ScheduleResponse> {
    const url = startDate 
      ? `/projects/${projectId}/schedule?start_date=${encodeURIComponent(startDate)}`
      : `/projects/${projectId}/schedule`;
    return this.request<ScheduleResponse>(url);
  }

  // --- Reports ---
  async getReport(projectId: string): Promise<ProjectReportResponse> {
    return this.request<ProjectReportResponse>(`/projects/${projectId}/report`);
  }
}

export const api = new ApiClient();
