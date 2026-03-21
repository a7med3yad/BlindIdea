import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { registerSchema, type RegisterFormData } from '../../../schemas/auth.schema';
import { useRegister } from '../../../hooks/useAuth';

export default function RegisterForm() {
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isValid },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: 'onChange',
  });

  const { mutate, isPending } = useRegister();
  const password = watch('password', '');

  const getPasswordStrength = (pwd: string) => {
    if (!pwd) return { level: 0, label: '', color: '' };
    let score = 0;
    if (pwd.length >= 8) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^a-zA-Z0-9]/.test(pwd)) score++;

    if (score <= 1) return { level: 1, label: 'Weak', color: '#EF4444' };
    if (score === 2) return { level: 2, label: 'Fair', color: '#F59E0B' };
    if (score === 3) return { level: 3, label: 'Good', color: '#F59E0B' };
    return { level: 4, label: 'Strong', color: '#22C55E' };
  };

  const strength = getPasswordStrength(password);

  const onSubmit = (data: RegisterFormData) => {
    mutate({ email: data.email, password: data.password });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Email"
        type="email"
        placeholder="you@example.com"

        error={errors.email?.message}
        disabled={isPending}
        {...register('email')}
      />

      <div>
        <Input
          label="Password"
          type="password"
          placeholder="Min. 8 characters"

          error={errors.password?.message}
          disabled={isPending}
          {...register('password')}
        />
        {password && (
          <div className="mt-2 space-y-1.5">
            <div className="flex gap-1.5">
              {[1, 2, 3, 4].map((seg) => (
                <div
                  key={seg}
                  className="h-1.5 flex-1 rounded-full transition-all duration-300"
                  style={{
                    backgroundColor:
                      seg <= strength.level ? strength.color : '#2A2A2A',
                  }}
                />
              ))}
            </div>
            <p className="text-xs font-medium" style={{ color: strength.color }}>
              {strength.label}
            </p>
          </div>
        )}
      </div>

      <Input
        label="Confirm Password"
        type="password"
        placeholder="Re-enter your password"

        error={errors.confirmPassword?.message}
        disabled={isPending}
        {...register('confirmPassword')}
      />

      <Button
        type="submit"
        fullWidth
        size="lg"
        isLoading={isPending}
        disabled={!isValid || isPending}
      >
        Create Account
      </Button>
    </form>
  );
}
