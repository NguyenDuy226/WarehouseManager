import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, PagingRequest } from '../Auth Service/user-servive';

export enum EntityStatus {
  Active = 1,
  Inactive = 0
}

export interface MaterialDTO {
  id: string;
  code: string;
  name: string;
  categoryId: string;
  categoryName: string;
  unitOfMeasureId: string;
  unitName: string;
  refPrice: number;
  mininumStock: number;
  status: EntityStatus;
}

export interface CreateMaterialDTO {
  name: string;
  categoryId: string;
  unitOfMeasureId: string;
  refPrice: number;
  mininumStock: number;
}

export interface UpdateMaterialDTO {
  name: string;
  categoryId: string;
  unitOfMeasureId: string;
  refPrice: number;
  mininumStock: number;
  status: EntityStatus;
}

@Injectable({
  providedIn: 'root',
})
export class MaterialService {
  private readonly apiUrl = `${environment.apiUrl}/Materials`;
  private readonly http = inject(HttpClient);

  getAll(request: PagingRequest): Observable<PagedResult<MaterialDTO>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.keyword) {
      params = params.set('keyword', request.keyword.trim());
    }
    if (request.sortBy) {
      params = params.set('sortBy', request.sortBy);
    }
    if (request.sortDirection) {
      params = params.set('sortDirection', request.sortDirection);
    }

    return this.http.get<PagedResult<MaterialDTO>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<MaterialDTO> {
    return this.http.get<MaterialDTO>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateMaterialDTO): Observable<MaterialDTO> {
    return this.http.post<MaterialDTO>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateMaterialDTO): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }

}