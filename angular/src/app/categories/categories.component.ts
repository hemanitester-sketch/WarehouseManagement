import { Component, OnInit, inject, ChangeDetectorRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { CategoryService } from '../proxy/products/category.service';
import { CategoryDto, CreateUpdateCategoryDto, CategoryLookupDto } from '../proxy/products/models';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss'],
})
export class CategoriesComponent implements OnInit {
  private categoryService = inject(CategoryService);
  private toasterService = inject(ToasterService);
  private cdr = inject(ChangeDetectorRef);

  categories: CategoryDto[] = [];
  filteredCategories: CategoryDto[] = [];
  parentCategoryOptions: CategoryDto[] = [];

  isLoading = false;
  isSaving = false;

  // Filters
  searchQuery = '';
  tierFilter: 'all' | 'root' | 'sub' = 'all';
  statusFilter: 'all' | 'active' | 'inactive' = 'all';

  // Action Menu Dropdown State
  openActionMenuId: string | null = null;

  // Tree Table Expand / Collapse State
  expandedCategoryIds = new Set<string>();

  toggleCategoryExpand(catId: string | undefined): void {
    if (!catId) return;
    if (this.expandedCategoryIds.has(catId)) {
      this.expandedCategoryIds.delete(catId);
    } else {
      this.expandedCategoryIds.add(catId);
    }
  }

  isCategoryExpanded(catId: string | undefined): boolean {
    return !!catId && this.expandedCategoryIds.has(catId);
  }

  getSubcategories(parentId: string | undefined): CategoryDto[] {
    if (!parentId) return [];
    const query = (this.searchQuery || '').trim().toLowerCase();
    return this.categories.filter((c) => {
      if (c.parentCategoryId !== parentId) return false;
      if (this.statusFilter === 'active' && !c.isActive) return false;
      if (this.statusFilter === 'inactive' && c.isActive) return false;
      if (query) {
        const parent = this.categories.find((p) => p.id === parentId);
        const parentMatches =
          parent &&
          (parent.code.toLowerCase().includes(query) ||
            parent.name.toLowerCase().includes(query) ||
            (parent.description && parent.description.toLowerCase().includes(query)));
        if (!parentMatches) {
          const childMatches =
            c.code.toLowerCase().includes(query) ||
            c.name.toLowerCase().includes(query) ||
            (c.description && c.description.toLowerCase().includes(query));
          if (!childMatches) return false;
        }
      }
      return true;
    });
  }

  getTotalSubcategoriesCount(parentId: string | undefined): number {
    if (!parentId) return 0;
    return this.categories.filter((c) => c.parentCategoryId === parentId).length;
  }

  hasSubcategories(cat: CategoryDto): boolean {
    if (!cat.id) return false;
    return this.getTotalSubcategoriesCount(cat.id) > 0;
  }

  toggleActionMenu(catId: string | undefined, event: MouseEvent): void {
    event.stopPropagation();
    if (!catId) return;
    this.openActionMenuId = this.openActionMenuId === catId ? null : catId;
  }

  closeActionMenu(): void {
    this.openActionMenuId = null;
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.openActionMenuId) {
      this.openActionMenuId = null;
    }
  }

  // Pagination State (50 Limit default)
  currentPage = 1;
  pageSize = 50;

  get paginatedCategories(): CategoryDto[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredCategories.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredCategories.length / this.pageSize) || 1;
  }

  get paginationStart(): number {
    return this.filteredCategories.length === 0 ? 0 : (this.currentPage - 1) * this.pageSize + 1;
  }

  get paginationEnd(): number {
    return Math.min(this.currentPage * this.pageSize, this.filteredCategories.length);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.currentPage = 1;
  }

  // Modal State
  isModalOpen = false;
  editingCategoryId: string | null = null;
  modalTitle = 'Register New Category';

  formModel: CreateUpdateCategoryDto = {
    code: '',
    name: '',
    description: '',
    parentCategoryId: null,
    isActive: true,
  };

  // Real-time Uniqueness Validation State
  isCodeDuplicate = false;
  codeStatusMessage: string | null = null;
  duplicateCategoryName = '';
  codeTouched = false;
  isCheckingCode = false;
  private codeCheckTimer: any = null;

  isNameDuplicate = false;
  nameStatusMessage: string | null = null;
  duplicateNameCode = '';
  nameTouched = false;
  isCheckingName = false;
  private nameCheckTimer: any = null;

  // Metrics
  totalCategories = 0;
  rootCategoriesCount = 0;
  subCategoriesCount = 0;
  activeCategoriesCount = 0;

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.categoryService.getList({ maxResultCount: 200, sorting: 'Code asc' }).subscribe({
      next: (res) => {
        this.categories = res.items || [];
        this.updateMetrics();
        this.updateParentOptions();
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading categories:', err);
        this.toasterService.error('Failed to load categories.', 'Error');
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
    this.currentPage = 1;
  }

  updateMetrics(): void {
    this.totalCategories = this.categories.length;
    this.rootCategoriesCount = this.categories.filter((c) => !c.parentCategoryId).length;
    this.subCategoriesCount = this.categories.filter((c) => !!c.parentCategoryId).length;
    this.activeCategoriesCount = this.categories.filter((c) => c.isActive).length;
  }

  updateParentOptions(): void {
    // Only root/parent categories can be selected as a parent category (prevents deep multi-level nest)
    this.parentCategoryOptions = this.categories.filter((c) => !c.parentCategoryId && c.isActive);
  }

  applyFilters(): void {
    this.openActionMenuId = null;
    const query = (this.searchQuery || '').trim().toLowerCase();

    // Hierarchical tree view: main grid rows are Root / Parent categories
    this.filteredCategories = this.categories.filter((cat) => {
      // Must be a parent (root) category
      const isRoot = !cat.parentCategoryId;
      if (!isRoot) return false;

      // 1. Tier filter
      if (this.tierFilter === 'sub' && !this.hasSubcategories(cat)) {
        return false;
      }

      // 2. Status filter
      let matchesStatus = true;
      if (this.statusFilter === 'active') {
        matchesStatus = cat.isActive;
      } else if (this.statusFilter === 'inactive') {
        matchesStatus = !cat.isActive;
      }

      // 3. Search query
      if (!query) {
        return matchesStatus;
      }

      const parentMatches =
        cat.code.toLowerCase().includes(query) ||
        cat.name.toLowerCase().includes(query) ||
        (cat.description && cat.description.toLowerCase().includes(query));

      const subcategories = this.categories.filter((c) => c.parentCategoryId === cat.id);
      const matchingChild = subcategories.some(
        (c) =>
          c.code.toLowerCase().includes(query) ||
          c.name.toLowerCase().includes(query) ||
          (c.description && c.description.toLowerCase().includes(query))
      );

      // If a child matches, auto-expand this parent so the matching child is seen
      if (matchingChild && cat.id) {
        this.expandedCategoryIds.add(cat.id);
      }

      return (parentMatches || matchingChild) && matchesStatus;
    });

    if (this.tierFilter === 'sub') {
      this.filteredCategories.forEach((cat) => {
        if (cat.id) this.expandedCategoryIds.add(cat.id);
      });
    }

    this.currentPage = 1;
  }

  openCreateModal(preselectedParent?: CategoryDto): void {
    this.editingCategoryId = null;
    this.modalTitle = preselectedParent
      ? `Add Subcategory under ${preselectedParent.name}`
      : 'Register New Category';

    this.formModel = {
      code: '',
      name: '',
      description: '',
      parentCategoryId: preselectedParent?.id || null,
      isActive: true,
    };

    this.resetValidationState();
    this.isModalOpen = true;
  }

  openEditModal(category: CategoryDto): void {
    this.editingCategoryId = category.id || null;
    this.modalTitle = `Edit Category: ${category.name}`;

    this.formModel = {
      code: category.code,
      name: category.name,
      description: category.description || '',
      parentCategoryId: category.parentCategoryId || null,
      isActive: category.isActive,
    };

    this.resetValidationState();
    this.isModalOpen = true;
  }

  private resetValidationState(): void {
    if (this.codeCheckTimer) clearTimeout(this.codeCheckTimer);
    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);

    this.isCodeDuplicate = false;
    this.codeStatusMessage = null;
    this.duplicateCategoryName = '';
    this.codeTouched = false;
    this.isCheckingCode = false;

    this.isNameDuplicate = false;
    this.nameStatusMessage = null;
    this.duplicateNameCode = '';
    this.nameTouched = false;
    this.isCheckingName = false;
  }

  closeModal(): void {
    this.resetValidationState();
    this.isModalOpen = false;
    this.editingCategoryId = null;
    this.isSaving = false;
  }

  // Real-time animated verification for Code
  onCodeInput(): void {
    this.codeTouched = true;
    const code = (this.formModel.code || '').trim().toUpperCase();

    if (this.codeCheckTimer) clearTimeout(this.codeCheckTimer);

    if (!code) {
      this.isCheckingCode = false;
      this.isCodeDuplicate = false;
      this.codeStatusMessage = null;
      this.duplicateCategoryName = '';
      return;
    }

    this.isCheckingCode = true;
    this.codeCheckTimer = setTimeout(() => {
      this.checkCodeUniqueness();
      this.isCheckingCode = false;
      this.cdr.detectChanges();
    }, 320);
  }

  onCodeBlur(): void {
    if (this.codeCheckTimer) clearTimeout(this.codeCheckTimer);
    const code = (this.formModel.code || '').trim().toUpperCase();
    if (code) {
      this.checkCodeUniqueness();
    }
    this.isCheckingCode = false;
  }

  checkCodeUniqueness(): void {
    this.codeTouched = true;
    const code = (this.formModel.code || '').trim().toUpperCase();

    if (!code) {
      this.isCodeDuplicate = false;
      this.codeStatusMessage = null;
      this.duplicateCategoryName = '';
      return;
    }

    const match = this.categories.find(
      (c) => c.code.toUpperCase() === code && c.id !== this.editingCategoryId
    );

    if (match) {
      this.isCodeDuplicate = true;
      this.duplicateCategoryName = match.name;
      this.codeStatusMessage = `Already exists`;
    } else {
      this.isCodeDuplicate = false;
      this.duplicateCategoryName = '';
      this.codeStatusMessage = `Available`;
    }
  }

  // Real-time animated verification for Name
  onNameInput(): void {
    this.nameTouched = true;
    const name = (this.formModel.name || '').trim();

    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);

    if (!name) {
      this.isCheckingName = false;
      this.isNameDuplicate = false;
      this.nameStatusMessage = null;
      this.duplicateNameCode = '';
      return;
    }

    this.isCheckingName = true;
    this.nameCheckTimer = setTimeout(() => {
      this.checkNameUniqueness();
      this.isCheckingName = false;
      this.cdr.detectChanges();
    }, 320);
  }

  onNameBlur(): void {
    if (this.nameCheckTimer) clearTimeout(this.nameCheckTimer);
    const name = (this.formModel.name || '').trim();
    if (name) {
      this.checkNameUniqueness();
    }
    this.isCheckingName = false;
  }

  checkNameUniqueness(): void {
    this.nameTouched = true;
    const name = (this.formModel.name || '').trim();

    if (!name) {
      this.isNameDuplicate = false;
      this.nameStatusMessage = null;
      this.duplicateNameCode = '';
      return;
    }

    const match = this.categories.find(
      (c) => c.name.trim().toLowerCase() === name.toLowerCase() && c.id !== this.editingCategoryId
    );

    if (match) {
      this.isNameDuplicate = true;
      this.duplicateNameCode = match.code;
      this.nameStatusMessage = `Already exists`;
    } else {
      this.isNameDuplicate = false;
      this.duplicateNameCode = '';
      this.nameStatusMessage = `Available`;
    }

    // Auto-suggest code if code hasn't been manually entered yet
    if (!this.editingCategoryId && (!this.formModel.code || !this.formModel.code.trim() || !this.codeTouched)) {
      this.suggestCodeFromName(name);
    }
  }

  suggestCodeFromName(name: string): void {
    if (!name || !name.trim()) return;

    const clean = name.trim().toUpperCase().replace(/[^A-Z0-9\s]/g, '');
    const words = clean.split(/\s+/).filter((w) => w.length > 0);

    let base = '';
    if (words.length === 1) {
      base = words[0].substring(0, 6);
    } else {
      base = words.map((w) => w.substring(0, 3)).join('').substring(0, 8);
    }
    if (!base) base = 'CAT';

    let candidate = base;
    let suffix = 1;
    while (this.categories.some((c) => c.code.toUpperCase() === candidate && c.id !== this.editingCategoryId)) {
      candidate = `${base.substring(0, Math.max(1, 7 - String(suffix).length))}${suffix}`;
      suffix++;
    }

    this.formModel.code = candidate;
    this.checkCodeUniqueness();
  }

  saveCategory(): void {
    this.checkCodeUniqueness();
    if (this.isCodeDuplicate) {
      this.toasterService.error(
        `A category with code '${this.formModel.code}' already exists. Please choose a unique code.`,
        'Duplicate Code'
      );
      return;
    }

    if (!this.formModel.code || !this.formModel.code.trim()) {
      this.toasterService.warn('Please enter a Category Code (e.g., PHARM, ELEC).', 'Validation');
      return;
    }

    if (!this.formModel.name || !this.formModel.name.trim()) {
      this.toasterService.warn('Please enter a Category Name.', 'Validation');
      return;
    }

    this.isSaving = true;
    const payload: CreateUpdateCategoryDto = {
      code: this.formModel.code.trim().toUpperCase(),
      name: this.formModel.name.trim(),
      description: this.formModel.description?.trim() || null,
      parentCategoryId: this.formModel.parentCategoryId || null,
      isActive: this.formModel.isActive,
    };

    const action$ = this.editingCategoryId
      ? this.categoryService.update(this.editingCategoryId, payload)
      : this.categoryService.create(payload);

    action$.subscribe({
      next: () => {
        this.toasterService.success(
          this.editingCategoryId ? 'Category updated successfully!' : 'Category created successfully!',
          'Success'
        );
        if (payload.parentCategoryId) {
          this.expandedCategoryIds.add(payload.parentCategoryId);
        }
        this.closeModal();
        this.loadCategories();
      },
      error: (err) => {
        console.error('Error saving category:', err);
        const msg = err?.error?.error?.message || 'Failed to save category. Please ensure Code is unique.';
        this.toasterService.error(msg, 'Error');
        this.isSaving = false;
        this.cdr.detectChanges();
      },
    });
    this.currentPage = 1;
  }

  deleteCategory(category: CategoryDto): void {
    if (category.subCategoriesCount > 0) {
      this.toasterService.error(
        `Cannot delete '${category.name}' because it contains ${category.subCategoriesCount} subcategory(ies). Please delete or reassign subcategories first.`,
        'Cannot Delete'
      );
      return;
    }

    const confirmed = confirm(
      `Are you sure you want to delete category '${category.name}' (${category.code})?`
    );
    if (!confirmed || !category.id) return;

    this.categoryService.delete(category.id).subscribe({
      next: () => {
        this.toasterService.success(`Category '${category.name}' deleted.`, 'Success');
        this.loadCategories();
      },
      error: (err) => {
        console.error('Error deleting category:', err);
        const msg = err?.error?.error?.message || 'Failed to delete category.';
        this.toasterService.error(msg, 'Error');
      },
    });
    this.currentPage = 1;
  }
}
