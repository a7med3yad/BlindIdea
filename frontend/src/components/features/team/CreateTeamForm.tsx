import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { createTeamSchema, type CreateTeamFormData } from '../../../schemas/team.schema';
import { useCreateTeam } from '../../../hooks/useTeam';

export default function CreateTeamForm() {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = useForm<CreateTeamFormData>({
    resolver: zodResolver(createTeamSchema),
    mode: 'onChange',
  });

  const { mutate, isPending } = useCreateTeam();

  const onSubmit = (data: CreateTeamFormData) => {
    mutate(data, { onSuccess: () => reset() });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Team Name"
        placeholder="Enter team name (3-50 characters)"

        error={errors.name?.message}
        disabled={isPending}
        {...register('name')}
      />
      <Button
        type="submit"
        fullWidth
        isLoading={isPending}
        disabled={!isValid || isPending}
      >
        Create Team
      </Button>
    </form>
  );
}
