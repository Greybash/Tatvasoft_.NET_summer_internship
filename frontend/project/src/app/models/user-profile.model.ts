// models/user-profile.model.ts
export interface UserDetail {
cityName: any;
countryName: any;
  id?: number;
  userId: number;
  name: string;
  surname: string;
  employeeId: string;
  manager: string;
  title: string;
  department: string;
  myProfile: string;
  whyIVolunteer: string;
  countryId: number;
  cityId: number;
  availability: string;
  linkedInUrl: string;
  mySkills: string;
  userImage: string;
  status: boolean;
  country?: Country;
  city?: City;
}

export interface Country {
  id: number;
  name: string;
  isActive: boolean;
}

export interface City {
  id: number;
  name: string;
  countryId: number;
  isActive: boolean;
}

export interface CreateUserDetailRequest {
  name: string;
  surname: string;
  employeeId: string;
  manager: string;
  title: string;
  department: string;
  myProfile: string;
  whyIVolunteer: string;
  countryId: number;
  cityId: number;
  availability: string;
  linkedInUrl: string;
  mySkills: string;
  status: boolean;
}

export interface UpdateUserDetailRequest {
  name: string;
  surname: string;
  employeeId: string;
  manager: string;
  title: string;
  department: string;
  myProfile: string;
  whyIVolunteer: string;
  countryId: number;
  cityId: number;
  availability: string;
  linkedInUrl: string;
  mySkills: string;
  status: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  error?: string;
}