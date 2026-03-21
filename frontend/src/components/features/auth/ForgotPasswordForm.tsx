import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import {
  forgotPasswordSchema,
  type ForgotPasswordFormData,
} from '../../../schemas/auth.schema';

interface ForgotPasswordFormProps {
  onSubmit: (email: string) => void;
  isLoading?: boolean;
}

export default function ForgotPasswordForm({
  onSubmit,
  isLoading = false,
}: ForgotPasswordFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<ForgotPasswordFormData>({
    resolver: zodResolver(forgotPasswordSchema),
    mode: 'onChange',
  });

  return (
    <form
      onSubmit={handleSubmit((data) => onSubmit(data.email))}
      className="space-y-5"
    >
      <Input
        label="Email"
        type="email"
        placeholder="Enter your email address"

        error={errors.email?.message}
        disabled={isLoading}
        {...register('email')}
      />

      <Button
        type="submit"
        fullWidth
        size="lg"
        isLoading={isLoading}
        disabled={!isValid || isLoading}
      >
        Send Reset Code
      </Button>
    </form>
  );
}
