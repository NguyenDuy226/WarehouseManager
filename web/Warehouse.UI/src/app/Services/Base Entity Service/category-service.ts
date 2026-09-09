import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EntityStatus } from './material-service';
import { environment } from '../../../environments/environment';
import { PagedResult, PagingRequest } from '../Auth Service/user-servive';

export interface MaterialCategoryDto {
  id: string;
  code: string;
  name: string;
  description: string;
  status: EntityStatus;
}

export interface CreateMaterialCategoryDto {
  name: string;
  description: string;
}

export interface UpdateMaterialCategoryDto {
  name: string;
  description: string;
  status: EntityStatus;
}

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private readonly apiUrl = `${environment.apiUrl}/MaterialCategories`;
  private readonly http = inject(HttpClient);

  getAll(request: PagingRequest): Observable<PagedResult<MaterialCategoryDto>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.keyword) params = params.set('keyword', request.keyword.trim());
    if (request.sortBy) params = params.set('sortBy', request.sortBy);
    if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);

    return this.http.get<PagedResult<MaterialCategoryDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<MaterialCategoryDto> {
    return this.http.get<MaterialCategoryDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateMaterialCategoryDto): Observable<MaterialCategoryDto> {
    return this.http.post<MaterialCategoryDto>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateMaterialCategoryDto): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
  
}