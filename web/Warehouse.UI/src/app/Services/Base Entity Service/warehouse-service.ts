import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../Auth Service/user-servive';


export enum WarehouseEntityStatus {
  Inactive = 0,
  Active = 1
}
export interface WarehouseDTO{
  id: string,
  code: string,
  name: string,
  address: string,
  manager: string,
  createdAt: string,
  status: WarehouseEntityStatus;
}
export interface UpdateWarehouseDTO{
  name: string,
  address: string,
  manager: string,
  status: WarehouseEntityStatus
}
export interface CreateWarehouseDTO{
  name: string,
  address: string,
  manager: string,
}
export interface PagingRequestWarehouse{
  pageNumber: number;
  pageSize: number;
  keyword?: string | null;
  sortBy?: string | null;        
  sortDirection?: string | null; 
  status?: string;
}

@Injectable({
  providedIn: 'root',
})
export class WarehouseService {
  private readonly apiUrl = `${environment.apiUrl}/warehouse`;
  private readonly http = inject(HttpClient);

  getall(request: PagingRequestWarehouse){
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
    if (request.status && request.status !== 'all') {
      params = params.set('status', request.status);
    }
    return this.http.get<PagedResult<WarehouseDTO>>(this.apiUrl, { params });
  }
  getById(id: string) {
    return this.http.get<WarehouseDTO>(`${this.apiUrl}/${id}`);
  } 
  update(id: string, request: UpdateWarehouseDTO) {
    return this.http.put<any>(`${this.apiUrl}/${id}`, request);
  }
  delete(id: string) {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
  create(request: CreateWarehouseDTO){
    return this.http.post<any>(`${this.apiUrl}`, request);
  }
  
}
