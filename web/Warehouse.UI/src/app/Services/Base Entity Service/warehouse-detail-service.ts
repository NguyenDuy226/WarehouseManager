import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../Auth Service/user-servive';

export interface WarehouseDetailRequest {
  warehouseId: string;
  pageNumber: number;
  pageSize: number;
  keyword?: string;
  sortBy?: string;
  sortDirection?: string;
}
export interface WarehouseDetailDTO {
  id: string;
  materialId: string;
  materialCode: string;
  materialName: string;
  unitOfMeasureName?: string;
  categoryName?: string;
  quantity: number;
  mininumStock: number;
}

@Injectable({
  providedIn: 'root',
})
export class WarehouseDetailService {
  private readonly apiUrl = `${environment.apiUrl}/WarehouseDetail`;
  private readonly http = inject(HttpClient);

  getDetail(request: WarehouseDetailRequest) {
    let params = new HttpParams()
      .set('warehouseId', request.warehouseId)
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
    return this.http.get<PagedResult<WarehouseDetailDTO>>(this.apiUrl, { params });
  
  }
}