  import { Injectable } from '@angular/core';
  import { HttpClient, HttpParams } from '@angular/common/http';
  import { Observable } from 'rxjs';
  import { environment } from '../../../environments/environment';
  import { PagedResult, PagingRequest } from '../Auth Service/user-servive'; 

  //enum
  export enum DocumentType {
    RECEIPT = 1,
    ISSUE = 2,
    TRANSFER = 3,
    ADJUSTMENT = 4,
    OPENING = 5,
    REVERSAL = 6
  }
  export enum DocumentStatus {
    DRAFT = 0,
    PENDING_APPROVAL = 1,
    POSTED = 2,
    REJECTED = 3,
    CANCELED = 4
  }

  //reponse
  export interface DocumentLineDTO {
    id?: string;
    materialId: string;
    itemCode?: string;
    itemName?: string;
    unit?: string;
    quantity: number;
    unitPrice: number;
  }

  export interface DocumentDTO {
    id: string;
    code: string;
    type: DocumentType;
    status: DocumentStatus;
    warehouseId: string;
    warehouseName?: string;
    createdAt: string;
    createdBy?: string;
    creatorName: string;
    reason?: string; 
    note?: string;

    //receipt
    supplierId?: string | null;
    supplierName?: string;
    externalDocumentNo?: string;

    //issue
    receiver?: string;

    //transfer
    toWarehouseId?: string | null;
    toWarehouseName?: string;

    lines?: DocumentLineDTO[];
  }

  //create
  export interface CreateDocumentBaseDTO {
    warehouseId: string;
    documentDate: string; 
    reason?: string;
    note?: string;
    lines: DocumentLineDTO[];
  }
  export interface CreateReceiptDTO extends CreateDocumentBaseDTO {
    supplierId?: string | null;
    externalDocumentNo?: string;
  }
  export interface CreateIssueDTO extends CreateDocumentBaseDTO {
    receiver?: string;
    externalDocumentNo?: string;
  }
  export interface CreateTransferDTO extends CreateDocumentBaseDTO {
    toWarehouseId: string;
  }
  export interface CreateAdjustmentDTO extends CreateDocumentBaseDTO {

  }
  export interface CreateOpeningDTO extends CreateDocumentBaseDTO {

  }
  export interface CreateReversalDTO extends CreateDocumentBaseDTO {
    originalDocumentId: string;
  }

  //update
  export interface UpdateDocumentBaseDTO {
    documentDate: string;
    reason?: string;
    note?: string;
    lines: DocumentLineDTO[];
  }
  export interface UpdateReceiptDTO extends UpdateDocumentBaseDTO {
    supplierId?: string | null;
    externalDocumentNo?: string;
  }
  export interface UpdateIssueDTO extends UpdateDocumentBaseDTO {
    receiver?: string;
    externalDocumentNo?: string;
  }
  export interface UpdateTransferDTO extends UpdateDocumentBaseDTO {
    toWarehouseId: string;
  }
  export interface UpdateAdjustmentDTO extends UpdateDocumentBaseDTO {

  }
  export interface UpdateOpeningDTO extends UpdateDocumentBaseDTO {

  }

  //work flow
  export interface RejectDocumentRequest {
    reason: string;
  }

  @Injectable({
    providedIn: 'root'
  })
  export class DocumentService {
    private readonly baseUrl = `${environment.apiUrl}/document`;

    constructor(private http: HttpClient) {}

    private cleanParam(request: PagingRequest): HttpParams {
      let params = new HttpParams()
        .set('pageNumber', request.pageNumber.toString())
        .set('pageSize', request.pageSize.toString());

      if (request.keyword) params = params.set('keyword', request.keyword);
      if (request.sortBy) params = params.set('sortBy', request.sortBy);
      if (request.sortDirection) params = params.set('sortDirection', request.sortDirection);
      if (request.status && request.status !== 'all') params = params.set('status', request.status);
      if (request.type && request.type !== 'all') params = params.set('type', request.type);

      return params;
    }

    //CRUD
    getAll(request: PagingRequest): Observable<PagedResult<DocumentDTO>> {
      const params = this.cleanParam(request);
      return this.http.get<PagedResult<DocumentDTO>>(this.baseUrl, { params });
    }

    getByUser(request: PagingRequest): Observable<PagedResult<DocumentDTO>> {
      const params = this.cleanParam(request);
      return this.http.get<PagedResult<DocumentDTO>>(`${this.baseUrl}/me`, { params });
    }

    getById(id: string): Observable<DocumentDTO> {
      return this.http.get<DocumentDTO>(`${this.baseUrl}/${id}`);
    }

    //create
    createReceipt(dto: CreateReceiptDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/receipt`, dto);
    }
    createIssue(dto: CreateIssueDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/issue`, dto);
    }
    createTransfer(dto: CreateTransferDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/transfer`, dto);
    }
    createAdjustment(dto: CreateAdjustmentDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/adjustment`, dto);
    }
    createOpening(dto: CreateOpeningDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/opening`, dto);
    }
    createReversal(dto: CreateReversalDTO): Observable<DocumentDTO> {
      return this.http.post<DocumentDTO>(`${this.baseUrl}/reversal`, dto);
    }

    //update
    updateReceipt(id: string, dto: UpdateReceiptDTO): Observable<void> {
      return this.http.put<void>(`${this.baseUrl}/${id}/receipt`, dto);
    }
    updateIssue(id: string, dto: UpdateIssueDTO): Observable<void> {
      return this.http.put<void>(`${this.baseUrl}/${id}/issue`, dto);
    }
    updateTransfer(id: string, dto: UpdateTransferDTO): Observable<void> {
      return this.http.put<void>(`${this.baseUrl}/${id}/transfer`, dto);
    }
    updateAdjustment(id: string, dto: UpdateAdjustmentDTO): Observable<void> {
      return this.http.put<void>(`${this.baseUrl}/${id}/adjustment`, dto);
    }
    updateOpening(id: string, dto: UpdateOpeningDTO): Observable<void> {
      return this.http.put<void>(`${this.baseUrl}/${id}/opening`, dto);
    }

    //work flow
    submit(id: string): Observable<void> {
      return this.http.post<void>(`${this.baseUrl}/${id}/submit`, {});
    }
    approve(id: string): Observable<void> {
      return this.http.post<void>(`${this.baseUrl}/${id}/approve`, {});
    }
    reject(id: string, reason: string): Observable<void> {
      const body: RejectDocumentRequest = { reason };
      return this.http.post<void>(`${this.baseUrl}/${id}/reject`, body);
    }
    cancel(id: string): Observable<void> {
      return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

  }