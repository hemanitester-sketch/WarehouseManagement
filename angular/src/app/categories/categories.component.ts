import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
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
    const query = (this.searchQuery || '').trim().toLowerCase();

    this.filteredCategories = this.categories.filter((cat) => {
      // 1. Text filter
      const matchesSearch =
        !query ||
        cat.code.toLowerCase().includes(query) ||
        cat.name.toLowerCase().includes(query) ||
        (cat.description && cat.description.toLowerCase().includes(query)) ||
        (cat.parentCategoryName && cat.parentCategoryName.toLowerCase().includes(query));

      // 2. Tier filter
      let matchesTier = true;
      if (this.tierFilter === 'root') {
        matchesTier = !cat.parentCategoryId;
      } else if (this.tierFilter === 'sub') {
        matchesTier = !!cat.parentCategoryId;
      }

      // 3. Status filter
      let matchesStatus = true;
      if (this.statusFilter === 'active') {
        matchesStatus = cat.isActive;
      } else if (this.statusFilter === 'inactive') {
        matchesStatus = !cat.isActive;
      }

      return matchesSearch && matchesTier && matchesStatus;
    });
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

    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.editingCategoryId = null;
    this.isSaving = false;
  }

  saveCategory(): void {
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
  }
}
