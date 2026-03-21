import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { loginSchema, type LoginFormData } from '../../../schemas/auth.schema';
import { useLogin } from '../../../hooks/useAuth';

export default function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: 'onChange',
  });

  const { mutate, isPending } = useLogin();

  const onSubmit = (data: LoginFormData) => {
    mutate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
      <Input
        label="Email"
        type="email"
        placeholder="you@example.com"

        error={errors.email?.message}
        disabled={isPending}
        {...register('email')}
      />

      <Input
        label="Password"
        type="password"
        placeholder="Enter your password"

        error={errors.password?.message}
        disabled={isPending}
        {...register('password')}
      />

      <Button
        type="submit"
        fullWidth
        size="lg"
        isLoading={isPending}
        disabled={!isValid || isPending}
      >
        Sign In
      </Button>
    </form>
  );
}
