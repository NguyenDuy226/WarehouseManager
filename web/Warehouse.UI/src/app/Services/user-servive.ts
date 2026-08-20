import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';

export interface UserDTO {
  id: string;
  code: string
  userName: string;
  email: string;
  name: string;
  isActive: boolean;
  createdAt?: string | Date;  
  roles?: string[];
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  keyword?: string | null;
  status?: string | null;        
  role?: string | null;          
  sortBy?: string | null;        
  sortDirection?: string | null; 
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPage: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface UpdateUserRequestDTO {
  name: string;
  isActive: boolean;
}

export interface RoleRequestDTO {
  role: string;
}

export interface WarehouseRequestDTO {
  warehouseId: string;
}

export interface UsersToWarehouseDTO {
  userId: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/users`;

  getAll(request: PagingRequest) {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.keyword) {
      params = params.set('keyword', request.keyword);
    }
    if (request.status && request.status !== 'all') {
      params = params.set('status', request.status);
    }
    if (request.role && request.role !== 'all') {
      params = params.set('role', request.role);
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }

    return this.http.get<PagedResult<UserDTO>>(this.apiUrl, { params });
  }

  getById(id: string) {
    return this.http.get<UserDTO>(`${this.apiUrl}/${id}`);
  }

  setRole(id: string, request: RoleRequestDTO) {
    return this.http.put<any>(`${this.apiUrl}/${id}/role`, request);
  }

  setWarehousesToUser(userId: string, request: WarehouseRequestDTO[]) {
    return this.http.post<any>(`${this.apiUrl}/${userId}/warehouse`, request);
  }

  setUsersToWarehouse(warehouseId: string, request: UsersToWarehouseDTO[]) {
    return this.http.post<any>(`${environment.apiUrl}/warehouses/${warehouseId}/users`, request);
  }

  update(id: string, request: UpdateUserRequestDTO) {
    return this.http.put<any>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
  
  getUserWarehouses(userId: string) {
    return this.http.get<string[]>(`${this.apiUrl}/${userId}/warehouses`);
  }
  getWarehouseUsers(warehouseId: string) {
    return this.http.get<string[]>(`${environment.apiUrl}/users/${warehouseId}/users`);
  }
  getValidUser(request: PagingRequest) {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.keyword) {
      params = params.set('keyword', request.keyword);
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }
    return this.http.get<PagedResult<UserDTO>>(`${environment.apiUrl}/users/valid-users`, { params });
  }

}