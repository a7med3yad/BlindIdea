import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

import Input from '../../ui/Input';
import Button from '../../ui/Button';
import { joinTeamSchema, type JoinTeamFormData } from '../../../schemas/team.schema';
import { useJoinTeam } from '../../../hooks/useTeam';

export default function JoinTeamForm() {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = useForm<JoinTeamFormData>({
    resolver: zodResolver(joinTeamSchema),
    mode: 'onChange',
  });

  const { mutate, isPending } = useJoinTeam();

  const onSubmit = (data: JoinTeamFormData) => {
    mutate(data, { onSuccess: () => reset() });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <Input
        label="Invite Code"
        placeholder="Enter 8-character code"

        error={errors.inviteCode?.message}
        disabled={isPending}
        {...register('inviteCode')}
        style={{ textTransform: 'uppercase' }}
      />
      <Button
        type="submit"
        fullWidth
        isLoading={isPending}
        disabled={!isValid || isPending}
      >
        Join Team
      </Button>
    </form>
  );
}
