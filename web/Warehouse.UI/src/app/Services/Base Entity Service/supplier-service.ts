import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EntityStatus } from './material-service';
import { environment } from '../../../environments/environment';
import { PagedResult, PagingRequest } from '../Auth Service/user-servive';

export interface SupplierDto {
  id: string;
  code: string;
  name: string;
  taxCode: string;
  addres: string; 
  contact: string;
  status: EntityStatus;
}

export interface CreateSupplierDto {
  name: string;
  taxCode: string;
  addres: string;
  contact: string;
}

export interface UpdateSupplierDto {
  name: string;
  taxCode: string;
  addres: string;
  contact: string;
  status: EntityStatus;
}

@Injectable({
  providedIn: 'root',
})
export class SupplierService {
  private readonly apiUrl = `${environment.apiUrl}/Suppliers`;
  private readonly http = inject(HttpClient);

  getAll(request: PagingRequest): Observable<PagedResult<SupplierDto>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.keyword) params = params.set('keyword', request.keyword.trim());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<PagedResult<SupplierDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<SupplierDto> {
    return this.http.get<SupplierDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateSupplierDto): Observable<SupplierDto> {
    return this.http.post<SupplierDto>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateSupplierDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
}