import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { combineLatest } from 'rxjs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../Services/Auth Service/auth-service';
import { DocumentService, DocumentDTO, DocumentType, DocumentStatus } from '../../Services/Document Service/document-service';
import { PagedResult, PagingRequest } from '../../Services/Auth Service/user-servive';
import { DocumentDetail } from './document-detail/document-detail';

@Component({
  selector: 'app-documents',
  imports: [RouterLink, CommonModule, ReactiveFormsModule, FormsModule, DocumentDetail],
  providers: [DatePipe],
  templateUrl: './documents.html',
})
export class Documents implements OnInit {
  private readonly documentService = inject(DocumentService);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  keyword: string = '';
  sortDirection: 'desc' | 'asc' = 'desc'; 
  sortBy: string = 'createdAt'; 
  pages: (number | string)[] = [];
  isLoading: boolean = false;

  isDocumentDetailOpen : boolean = false;
  selectedDocumentId: string | null = null;


  pagedResult: PagedResult<DocumentDTO> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPage: 1,
    hasPreviousPage: false,
    hasNextPage: false
  };
  role: string | string[] | null = null;
  isManagerOrAdmin : boolean = false; 
  documentStatus = DocumentStatus;

  ngOnInit(): void {
    this.role = this.authService.getRoleFromToken();
    this.checkRole();

    combineLatest([
      this.route.paramMap,
      this.route.queryParamMap
    ])
    .subscribe(([params, queryParams]) => {
      const pageParam = params.get('page');
      const parsedPage = pageParam ? parseInt(pageParam, 10) : 1;
      this.pagedResult.pageNumber = !isNaN(parsedPage) && parsedPage > 0 ? parsedPage : 1;
      this.keyword = queryParams.get('keyword') || '';
      this.sortBy = queryParams.get('sortBy') || 'createdAt';
      this.sortDirection = (queryParams.get('sortDirection') as 'asc' | 'desc') || 'desc';
      this.loadDocuments();
    });
  }
  
  loadDocuments() {
    this.isLoading = true;
    const request: PagingRequest = {
      pageNumber: this.pagedResult.pageNumber,
      pageSize: this.pagedResult.pageSize,
      keyword: this.keyword,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection        
    };
    if(this.isManagerOrAdmin){
      this.documentService.getAll(request).subscribe({
        next: (res) => {
          this.pagedResult = res;
          this.calculatePage();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Lỗi khi tải danh sách chứng từ:', err);
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
    else{
      this.documentService.getByUser(request).subscribe({
        next: (res) => {
          this.pagedResult = res;
          this.calculatePage();
          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Lỗi khi tải danh sách chứng từ:', err);
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
    
  }

  //paging 
  private cleanParams() {
    return {
      keyword: this.keyword ? this.keyword : null,
      sortBy: this.sortBy,
      sortDirection: this.sortDirection
    };
  }
  goToPage(page: number | string): void {
    if (typeof page === 'number' && page >= 1 && page <= this.pagedResult.totalPage) {
      this.router.navigate(['/documents/page', page], { queryParams: this.cleanParams() });
    }
  }
  nextPage(): void {
    if (this.pagedResult.hasNextPage) this.goToPage(this.pagedResult.pageNumber + 1);
  }
  previousPage(): void {
    if (this.pagedResult.hasPreviousPage) this.goToPage(this.pagedResult.pageNumber - 1);
  }
  private calculatePage(): void {
    const total = this.pagedResult.totalPage; 
    const current = this.pagedResult.pageNumber;
    const pages: (number | string)[] = [];
    
    if (total <= 7) {
      for (let i = 1; i <= total; i++) pages.push(i);
    } 
    else {
      pages.push(1);
      if (current > 3) pages.push('...');
      const start = Math.max(2, current - 1);
      const end = Math.min(total - 1, current + 1);
      for (let i = start; i <= end; i++) pages.push(i);
      if (current < total - 2) pages.push('...');
      pages.push(total);
    }
    this.pages = pages;
  }

  //sort and search
  sort(column: string): void {
    if (this.sortBy === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = column;
      this.sortDirection = 'asc';
    }
    this.router.navigate(['/documents/page/1'], { queryParams: this.cleanParams() });
  }
  search(): void {
    this.router.navigate(['/documents/page/1'], { queryParams: this.cleanParams() });
  }
  
  //action
  checkRole() {
    if (!this.role) {
      this.isManagerOrAdmin  = false;
      return;
    }
    const roles = Array.isArray(this.role) ? this.role : [this.role];
    this.isManagerOrAdmin = roles.some(r => r.toUpperCase() === 'SYSTEM_ADMIN' || r.toUpperCase() === 'WAREHOUSE_MANAGER' || r.toUpperCase() === 'APPROVER');
  }
  approveDoc(id: string, code: string): void {
    if (confirm(`Bạn có chắc chắn muốn DUYỆT phiếu [${code}] không?`)) {
      this.isLoading = true;
      this.documentService.approve(id).subscribe({
        next: () => {
          this.loadDocuments();
        },
        error: (err) => {
          alert(err.error?.message || 'Có lỗi xảy ra khi duyệt phiếu!');
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }
  rejectDoc(id: string, code: string): void {
    const reason = prompt(`Nhập lý do từ chối cho phiếu [${code}]:`);
    if (reason === null) return;
    if (reason.trim() === '') {
      alert('Vui lòng nhập lý do từ chối!');
      return;
    }
    this.isLoading = true;
    this.documentService.reject(id, reason).subscribe({
      next: () => {
        this.loadDocuments();
      },
      error: (err) => {
        alert(err.error?.message || 'Có lỗi xảy ra khi từ chối phiếu!');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
  cancelDoc(id: string, code: string): void{
    if (confirm(`Bạn có chắc chắn muốn TỪ CHỐI phiếu [${code}] không?`)) {
      this.isLoading = true;
      this.documentService.cancel(id).subscribe({
        next: () => {
          this.loadDocuments();
        },
        error: (err) => {
          alert(err.error?.message || 'Có lỗi xảy ra khi từ chối phiếu!');
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      });
    }
  }  
  getTypeName(type: DocumentType): string {
    const types: Record<number, string> = {
      1: 'Nhập kho', 
      2: 'Xuất kho', 
      3: 'Chuyển kho',
      4: 'Điều chỉnh', 
      5: 'Đầu kỳ', 
      6: 'Phiếu đảo'
    };
    return types[type] || 'Khác';
  }

  //child component
  openDetail(id: string): void {
    this.selectedDocumentId = id;
    this.isDocumentDetailOpen = true;
    this.cdr.detectChanges();
  }
  closeDetail(): void {
    this.selectedDocumentId = null;
    this.isDocumentDetailOpen = false;
    this.cdr.detectChanges();
  }
  handleDetail(event: { id: string, newStatus: DocumentStatus }): void {
    const index = this.pagedResult.items.findIndex(doc => doc.id === event.id);
    if (index !== -1) {
      this.pagedResult.items[index].status = event.newStatus;
      this.cdr.detectChanges();
    }
  }  

}