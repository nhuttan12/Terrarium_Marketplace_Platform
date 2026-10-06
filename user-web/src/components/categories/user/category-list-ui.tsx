'use client';

import { CategoryItem } from '@/types/categories/user/CategoryItem';
import { renderIcon } from '@/utils/categories/mappers/user-categories';
import Link from 'next/link';
import { JSX } from 'react';

export interface CategoryListUiProps {
	categories: CategoryItem[];
}

export default function CategoryListUi({ categories }: CategoryListUiProps): JSX.Element {
	return (
		<section className='w-full p-6 mx-auto bg-white border max-w-7xl rounded-2xl border-slate-200'>
			<div className='mb-4'>
				<h2 className='text-lg font-bold text-slate-800'>Danh mục sản phẩm</h2>
			</div>

			<div className='flex gap-4 pb-4 overflow-x-auto lg:grid lg:grid-cols-5 lg:gap-6 lg:overflow-visible lg:pb-0'>
				{categories.map((category) => (
					<Link
						key={category.id}
						href={category.href}
						className='flex flex-col items-center flex-shrink-0 w-20 gap-3 cursor-pointer group lg:w-auto'
					>
						<div className='flex items-center justify-center w-14 h-14 transition-all duration-300 rounded-2xl bg-emerald-50 text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white group-hover:shadow-md group-hover:scale-105'>
							{renderIcon(category.iconName)}
						</div>
						{/* Tên danh mục */}
						<span className='text-xs font-medium text-center transition-colors text-slate-600 group-hover:text-emerald-700 line-clamp-2'>
							{category.name}
						</span>
					</Link>
				))}
			</div>
		</section>
	);
}
